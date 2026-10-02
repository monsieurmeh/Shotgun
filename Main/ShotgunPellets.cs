using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.AI;
using Il2CppTLD.Gear;
using MelonLoader;
using UnityEngine;

namespace Shotgun
{
    internal static class ShotgunPellets
    {
        internal enum ShotgunAmmo { None, Slug, Buckshot, Birdshot }

        internal const bool PelletsCanBleed = false;

        internal const int BulletLayerMask = 0x180e1b41;
        internal const int GearLayer = 17;

        internal static bool InShotgunShot { get; private set; }
        internal static ShotgunAmmo CurrentAmmo { get; private set; } = ShotgunAmmo.None;
        private static bool mShotHitAnimal;

        internal static bool DebugLog = false;

        private static readonly Dictionary<IntPtr, PelletTarget> mTargets = new Dictionary<IntPtr, PelletTarget>();


        internal sealed class PelletTarget
        {
            internal BaseAi Ai;
            internal int Pellets;
            internal float Damage;
            internal float BleedMinutes;
            internal LocalizedDamage HeaviestPart;
            internal float HeaviestPartDamage;
            internal string ColliderName = string.Empty;
        }


        internal static ShotgunAmmo GetLoadedAmmo()
        {
            PlayerManager playerManager = GameManager.GetPlayerManagerComponent();
            if (playerManager == null) return ShotgunAmmo.None;
            if (playerManager.m_ItemInHands == null) return ShotgunAmmo.None;
            GunItem gun = playerManager.m_ItemInHands.m_GunItem;
            if (gun == null) return ShotgunAmmo.None;
            if (gun.m_GunType != GunTypeCustom) return ShotgunAmmo.None;

            GearItemData ammoData;
            if (!gun.TryGetSelectedAmmo(out ammoData)) return ShotgunAmmo.None;
            if (ammoData == null) return ShotgunAmmo.None;

            string ammoName = ammoData.name;
            if (ammoName.Contains("Slug")) return ShotgunAmmo.Slug;
            if (ammoName.Contains("Buckshot")) return ShotgunAmmo.Buckshot;
            if (ammoName.Contains("Birdshot")) return ShotgunAmmo.Birdshot;
            return ShotgunAmmo.None;
        }


        internal static bool IsPelletAmmo(ShotgunAmmo ammo) => ammo == ShotgunAmmo.Buckshot || ammo == ShotgunAmmo.Birdshot;


        internal static int GetPelletCount(ShotgunAmmo ammo)
        {
            switch (ammo)
            {
                case ShotgunAmmo.Buckshot: return Settings.Instance.BuckshotPelletCount;
                case ShotgunAmmo.Birdshot: return Settings.Instance.BirdshotPelletCount;
                default: return 1;
            }
        }


        internal static float GetPelletSpread(ShotgunAmmo ammo)
        {
            switch (ammo)
            {
                case ShotgunAmmo.Buckshot: return Settings.Instance.BuckshotPelletSpread / 2;
                case ShotgunAmmo.Birdshot: return Settings.Instance.BirdshotPelletSpread / 2;
                default: return 0f;
            }
        }


        internal static float GetFalloffPerMeter(ShotgunAmmo ammo)
        {
            switch (ammo)
            {
                case ShotgunAmmo.Slug: return Settings.Instance.SlugDamageFalloff;
                case ShotgunAmmo.Buckshot: return Settings.Instance.BuckshotDamageFalloff;
                case ShotgunAmmo.Birdshot: return Settings.Instance.BirdshotDamageFalloff;
                default: return 0f;
            }
        }


        internal static float GetMinimumDamage(ShotgunAmmo ammo)
        {
            switch (ammo)
            {
                case ShotgunAmmo.Slug: return Settings.Instance.SlugMinimumDamage;
                case ShotgunAmmo.Buckshot: return Settings.Instance.BuckshotMinimumDamage;
                case ShotgunAmmo.Birdshot: return Settings.Instance.BirdshotMinimumDamage;
                default: return 0f;
            }
        }


        internal static void BeginShot(ShotgunAmmo ammo)
        {
            InShotgunShot = true;
            CurrentAmmo = ammo;
            mShotHitAnimal = false;
        }


        internal static void EndShot()
        {
            if (!InShotgunShot) return;
            if (mShotHitAnimal) IncreaseSkillForShot();
            InShotgunShot = false;
            CurrentAmmo = ShotgunAmmo.None;
            mShotHitAnimal = false;
        }


        internal static void MarkHitAnimal() => mShotHitAnimal = true;


        private static void IncreaseSkillForShot()
        {
            if (ShotgunSkill.IncreaseSkillPoints == null) return;
            ShotgunSkill.IncreaseSkillPoints.Invoke(ShotgunSkill.SkillName, 1);
        }


        internal static void ResolvePellets(vp_Bullet bullet, ShotgunAmmo ammo)
        {
            Transform bulletTransform = bullet.transform;
            Vector3 origin = bulletTransform.position;
            Quaternion aim = bulletTransform.rotation;
            Transform playerTransform = GameManager.GetPlayerTransform();
            if (playerTransform == null)
            {
                UnityEngine.Object.Destroy(bullet.gameObject);
                return;
            }
            Vector3 playerPosition = playerTransform.position;
            int pelletCount = GetPelletCount(ammo);
            float spread = GetPelletSpread(ammo);
            float falloffPerMeter = GetFalloffPerMeter(ammo);
            float minimumDamage = GetMinimumDamage(ammo);
            bool anyHit = false;
            RaycastHit firstHit = default;

            CougarManager cougarManager = GameManager.GetCougarManager();
            if (cougarManager != null) cougarManager.MaybeAimNearMiss(bulletTransform.forward);

            mTargets.Clear();
            MatchTransform.EnableCollidersForAllActive(true);
            try
            {
                for (int i = 0; i < pelletCount; i++)
                {
                    float roll = UnityEngine.Random.Range(0, 360);
                    float yaw = UnityEngine.Random.Range(-spread, spread);
                    Vector3 direction = aim * Quaternion.Euler(0f, 0f, roll) * Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;

                    RaycastHit hit;
                    if (!AiUtils.RaycastWithAimAssist(origin, direction, out hit, bullet.Range, bullet.MinDistanceForAimAssist, bullet.Accuracy, 0f, BulletLayerMask)) continue;
                    if (hit.collider == null) continue;

                    if (!anyHit)
                    {
                        anyHit = true;
                        firstHit = hit;
                    }
                    bullet.SpawnImpactEffects(hit);

                    BaseAi ai = AiUtils.GetBaseAiFromObject(hit.collider.gameObject);
                    if (ai == null || ai.GetAiMode() == AiMode.Dead)
                    {
                        MaybePushGear(bullet, hit, pelletCount);
                        continue;
                    }

                    LocalizedDamage part = hit.collider.GetComponent<LocalizedDamage>();
                    if (part == null) continue;

                    AddPelletHit(bullet, ai, part, hit, playerPosition, falloffPerMeter, minimumDamage);
                }

                foreach (PelletTarget target in mTargets.Values)
                {
                    ApplyToTarget(target, playerPosition);
                }
            }
            finally
            {
                mTargets.Clear();
                MatchTransform.EnableCollidersForAllActive(false);
                FinishBullet(bullet, anyHit, firstHit);
            }
        }


        private static void AddPelletHit(vp_Bullet bullet, BaseAi ai, LocalizedDamage part, RaycastHit hit, Vector3 playerPosition, float falloffPerMeter, float minimumDamage)
        {
            float bleed = part.GetBleedOutMinutes(WeaponSource.Unspecified);
            float damage = bullet.Damage * part.GetDamageScale(WeaponSource.Unspecified);
            float distance = Vector3.Distance(playerPosition, hit.collider.transform.position);
            if (distance >= bullet.Accuracy)
            {
                damage -= (distance - bullet.Accuracy) * falloffPerMeter;
                if (damage < minimumDamage) damage = minimumDamage;
            }

            PelletTarget target;
            if (!mTargets.TryGetValue(ai.Pointer, out target))
            {
                target = new PelletTarget { Ai = ai };
                mTargets[ai.Pointer] = target;
            }
            target.Pellets++;
            target.Damage += damage;
            if (bleed > 0f && (target.BleedMinutes <= 0f || bleed < target.BleedMinutes)) target.BleedMinutes = bleed;
            if (target.HeaviestPart == null || damage > target.HeaviestPartDamage)
            {
                target.HeaviestPart = part;
                target.HeaviestPartDamage = damage;
                target.ColliderName = hit.collider.name;
            }
        }


        private static void ApplyToTarget(PelletTarget target, Vector3 playerPosition)
        {
            BaseAi ai = target.Ai;
            if (ai == null) return;
            if (ai.GetAiMode() == AiMode.Dead) return;
            if (target.HeaviestPart == null) return;
            if (Mathf.Abs(target.Damage) <= 0.0001f && ai.m_HuntedPart2WaypointController == null) return;

            ai.SetupDamageForAnim(target.HeaviestPart.transform.position, playerPosition, target.HeaviestPart);
            ai.ApplyDamage(target.Damage, target.BleedMinutes, DamageSource.Player, target.ColliderName);
            ai.MaybeFleeOrAttackFromProjectileHit(WeaponSource.Unspecified);

            if (DebugLog)
                MelonLogger.Msg($"[Pellets] {ai.name}: {target.Pellets} pellet(s), {target.Damage:F1} dmg, bleed {target.BleedMinutes:F0}m, worst part {target.ColliderName}, hp now {ai.m_CurrentHP:F1}");
        }


        private static void MaybePushGear(vp_Bullet bullet, RaycastHit hit, int pelletCount)
        {
            GameObject hitObject = hit.collider.gameObject;
            if (hitObject.layer != GearLayer) return;
            GearItem gearItem = hitObject.GetComponent<GearItem>();
            if (gearItem == null) return;

            float scale = 1f / Mathf.Max(1, pelletCount);
            Vector3 force = (-hit.normal * bullet.m_GearImpactForce + Vector3.up * UnityEngine.Random.Range(0f, bullet.m_GearImpactUpwardForce)) * scale;
            float torque = bullet.m_GearImpactTorqueForce;
            Vector3 torqueVector = new Vector3(UnityEngine.Random.Range(-torque, torque), UnityEngine.Random.Range(-torque, torque), UnityEngine.Random.Range(-torque, torque)) * scale;
            gearItem.ApplyForce(force, torqueVector);
        }


        private static void FinishBullet(vp_Bullet bullet, bool anyHit, RaycastHit firstHit)
        {
            GameObject bulletObject = bullet.gameObject;
            if (!anyHit || firstHit.collider == null)
            {
                UnityEngine.Object.Destroy(bulletObject);
                return;
            }

            bulletObject.transform.position = firstHit.point;
            bulletObject.transform.SetParent(firstHit.transform, true);
            Renderer renderer = bulletObject.GetComponent<Renderer>();
            if (renderer != null && renderer.enabled)
            {
                vp_DecalManager.Add(bulletObject);
                return;
            }
            UnityEngine.Object.Destroy(bulletObject);
        }


        [HarmonyPatch(typeof(vp_Bullet), nameof(vp_Bullet.Start))]
        internal static class vp_Bullet_ResolveShotgunShot
        {
            internal static bool Prefix(vp_Bullet __instance)
            {
                if (__instance.m_GunType != GunTypeCustom) return true;

                ShotgunAmmo ammo = GetLoadedAmmo();
                BeginShot(ammo);

                __instance.DamageFalloffPerMeterBeyondEffectiveRange = GetFalloffPerMeter(ammo);
                __instance.MinimumDamageFalloffBeyondEffectiveRange = GetMinimumDamage(ammo);

                if (!IsPelletAmmo(ammo)) return true;

                try
                {
                    ResolvePellets(__instance, ammo);
                }
                catch (Exception e)
                {
                    MelonLogger.Error($"Pellet resolution failed: {e}");
                }
                return false;
            }

            internal static void Postfix(vp_Bullet __instance)
            {
                if (__instance == null) return;
                EndShot();
            }
        }


        [HarmonyPatch(typeof(LocalizedDamage), nameof(LocalizedDamage.GetBleedOutMinutes), new Type[] { typeof(WeaponSource) })]
        internal static class LocalizedDamage_ShotgunBleed
        {
            internal static void Postfix(ref float __result)
            {
                if (!InShotgunShot) return;
                MarkHitAnimal();
                if (IsPelletAmmo(CurrentAmmo) && !PelletsCanBleed) __result = 0f;
            }
        }


        [HarmonyPatch(typeof(LocalizedDamage), nameof(LocalizedDamage.RollChanceToKill), new Type[] { typeof(WeaponSource) })]
        internal static class LocalizedDamage_ShotgunCriticals
        {
            internal static void Postfix(LocalizedDamage __instance, ref bool __result)
            {
                if (!InShotgunShot) return;
                if (IsPelletAmmo(CurrentAmmo))
                {
                    __result = false;
                    return;
                }
                if (CurrentAmmo != ShotgunAmmo.Slug) return;

                float shotgunTierCritMultiplier = ShotgunSkill.GetCurrentTier() * 0.25f;
                float adjustedChance = __instance.GetChanceKill(WeaponSource.Rifle) * shotgunTierCritMultiplier * 1.25f;
                if (DebugLog) MelonLogger.Msg($"[Pellets] slug crit chance {adjustedChance}");
                __result = Utils.RollChance(adjustedChance);
            }
        }


        [HarmonyPatch(typeof(ConsoleManager), nameof(ConsoleManager.Initialize))]
        internal static class ConsoleManager_RegisterPelletLog
        {
            private static void Postfix()
            {
                uConsole.RegisterCommand("shotgun_pellet_log", new Action(() =>
                {
                    DebugLog = !DebugLog;
                    MelonLogger.Msg($"Shotgun pellet log: {DebugLog}");
                }));
            }
        }
    }
}
