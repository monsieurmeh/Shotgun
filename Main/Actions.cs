global using static Shotgun.Actions;
using Il2Cpp;
using MelonLoader;
using Shotgun.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Shotgun
{
    internal class Actions
    {

        internal static void SyncAmmoParams(GunItem gunItem)
        {
            if (gunItem == null || gunItem.m_Clip == null) return;
            Animator moddedAnimator = Main.Instance?.Configuration?.Refs?.ModdedAnimator;
            if (moddedAnimator == null) return;
            moddedAnimator.SetInteger("Rounds_Loaded", gunItem.m_Clip.Count);
            moddedAnimator.SetInteger("SpentCasingsInClip", gunItem.m_SpentCasingsInClip);
        }

        internal static void UpdateShotgunParams(GunItem gun, string ammoName)
        {
            if (gun == null) return;
            if (!gun.name.Contains("Shotgun")) return;
            vp_Bullet currentBullet = Main.Instance.Configuration.Refs.Shotgun.WeaponView.FPSShooter.ProjectilePrefab.GetComponent<vp_Bullet>();
            if (ammoName.Contains("Slug"))
            {
                gun.m_DamageHP = Settings.Instance.SlugBaseDamage;
                gun.m_AccuracyRange = Settings.Instance.SlugAccuracyRange;
                Main.Instance.Configuration.Refs.Shotgun.WeaponView.FPSShooter.ProjectileCount = 1;
                Main.Instance.Configuration.Refs.Shotgun.WeaponView.FPSShooter.ProjectileSpread = 0f;
                currentBullet.MinimumDamageFalloffBeyondEffectiveRange = Settings.Instance.SlugMinimumDamage;
                currentBullet.DamageFalloffPerMeterBeyondEffectiveRange = Settings.Instance.SlugDamageFalloff;
            }
            else if (ammoName.Contains("Buckshot"))
            {
                gun.m_DamageHP = Settings.Instance.BuckshotBaseDamage;
                gun.m_AccuracyRange = Settings.Instance.BuckshotAccuracyRange;
                Main.Instance.Configuration.Refs.Shotgun.WeaponView.FPSShooter.ProjectileCount = Settings.Instance.BuckshotPelletCount;
                Main.Instance.Configuration.Refs.Shotgun.WeaponView.FPSShooter.ProjectileSpread = Settings.Instance.BuckshotPelletSpread / 2;
                currentBullet.MinimumDamageFalloffBeyondEffectiveRange = Settings.Instance.BuckshotMinimumDamage;
                currentBullet.DamageFalloffPerMeterBeyondEffectiveRange = Settings.Instance.BuckshotDamageFalloff;
            }
            else if (ammoName.Contains("Birdshot"))
            {
                gun.m_DamageHP = Settings.Instance.BirdshotBaseDamage;
                gun.m_AccuracyRange = Settings.Instance.BirdshotAccuracyRange;
                Main.Instance.Configuration.Refs.Shotgun.WeaponView.FPSShooter.ProjectileCount = Settings.Instance.BirdshotPelletCount;
                Main.Instance.Configuration.Refs.Shotgun.WeaponView.FPSShooter.ProjectileSpread = Settings.Instance.BirdshotPelletSpread / 2;
                currentBullet.MinimumDamageFalloffBeyondEffectiveRange = Settings.Instance.BirdshotMinimumDamage;
                currentBullet.DamageFalloffPerMeterBeyondEffectiveRange = Settings.Instance.BirdshotDamageFalloff;
            }
        }

        internal static void TransmitLocomotion(PlayerMovement __instance)
        {
            Animator moddedAnimator = Main.Instance?.Configuration?.Refs?.ModdedAnimator;
            if (moddedAnimator == null || !moddedAnimator.enabled) return;

            bool sprint = InputManager.GetSprintDown(__instance);
            Vector2 move = InputManager.GetPlayerMovement(__instance);
            bool hasInput = move.sqrMagnitude > 0.0001f;
            Transform playerTransform = GameManager.GetPlayerTransform();
            float speed = 0f;

            if (playerTransform != null && Time.deltaTime > 0f)
            {
                if (Main.Instance.LastPosisiton != Vector3.zero)
                {
                    Vector3 d = playerTransform.position - Main.Instance.LastPosisiton;
                    d.y = 0f;
                    speed = d.magnitude / Time.deltaTime;
                }
                Main.Instance.LastPosisiton = playerTransform.position;
            }

            moddedAnimator.SetFloat("Movement_Speed", speed, 0.10f, Time.deltaTime);
            moddedAnimator.SetBool("Movement_Input", hasInput);
            moddedAnimator.SetBool("Sprint_Input", sprint);
            moddedAnimator.SetFloat("Idle_Speed_Multiplier", Settings.Instance.IdleSpeedMulitplier);
            moddedAnimator.SetFloat("Walk_Speed_Multiplier", Settings.Instance.BaseWalkingSpeedMulitplier * Mathf.Lerp(1.0f, Settings.Instance.MaxWalkingSpeedMulitplier, 1.0f - ((Settings.Instance.MaxWalkingSpeed - speed) / (Settings.Instance.MaxWalkingSpeed))));
            moddedAnimator.SetFloat("Sprint_Speed_Multiplier", Settings.Instance.BaseSprintingSpeedMulitplier * Mathf.Lerp(1.0f, Settings.Instance.MaxSprintingSpeedMulitplier, 1.0f - ((Settings.Instance.MaxSprintingSpeed - speed) / (Settings.Instance.MaxSprintingSpeed - Settings.Instance.MaxWalkingSpeed))));
            moddedAnimator.SetBool("Fast_Enough_For_Sprinting", speed >= Settings.Instance.MaxWalkingSpeed);

            // move to settings post-set, this is spamming here
            Main.Instance.IdleVariation.TimeScale = Settings.Instance.IdleVariationSpeed;
            Main.Instance.IdleVariation.MinSegmentDuration = Settings.Instance.IdleVariationMinInterval;
            Main.Instance.IdleVariation.MaxSegmentDuration = Settings.Instance.IdleVariationMaxInterval;
            Main.Instance.IdleVariation.MinTargetDistance = Settings.Instance.IdleVariationMinTargetDistance;
            Main.Instance.IdleVariation.MaxTargetDistance = Settings.Instance.IdleVariationMaxTargetDistance;
            Main.Instance.IdleVariation.OutputMin = Settings.Instance.IdleVariationOutputMin;
            Main.Instance.IdleVariation.OutputMax = Settings.Instance.IdleVariationOutputMax;
            Main.Instance.IdleVariation.Tick();

            moddedAnimator.SetFloat("Idle_Variation", Main.Instance.IdleVariation.Value);
        }


        internal static bool MaybeLatchShotgunAnimator(PlayerAnimation instance, string callerName = "")
        {
            Animator vanillaAnimator = Main.Instance.Configuration.Refs.VanillaAnimator;
            if (vanillaAnimator == null)
            {
                return false;
            }

            Animator moddedAnimator = Main.Instance.Configuration.Refs.ModdedAnimator;
            if (moddedAnimator == null)
            {
                vanillaAnimator.enabled = true;
                return false;
            }

            if (instance.m_EquippedFirstPersonWeaponShoulder == null)
            {
                moddedAnimator.enabled = false;
                vanillaAnimator.enabled = true;
                return false;
            }
            Animator shotgunAnimator = instance.m_EquippedFirstPersonWeaponShoulder.m_Animator;

            if (shotgunAnimator == null)
            {
                moddedAnimator.enabled = false;
                vanillaAnimator.enabled = true;
                return false;
            }

            bool isShotgun = instance.m_EquippedFirstPersonWeaponShoulder.name.Contains("Shotgun");

            moddedAnimator.enabled = isShotgun;
            vanillaAnimator.enabled = !isShotgun;
            shotgunAnimator.enabled = !isShotgun;

            return isShotgun;
        }

        internal static void ApplyShotgunRecoil()
        {
            float pitchMax = Settings.Instance.RecoilMaxPitchKick;
            float yawMax = Settings.Instance.RecoilMaxYawKick;
            if (pitchMax <= 0f && yawMax <= 0f) return;

            int tier = ShotgunSkill.GetCurrentTier();
            float factor = Mathf.Clamp01(1f - (tier / 4f));
            if (factor <= 0f) return;

            MelonCoroutines.Start(RecoilRoutine(pitchMax * factor, UnityEngine.Random.Range(-yawMax, yawMax) * factor));
        }

        private static System.Collections.IEnumerator RecoilRoutine(float pitchTotal, float yawTotal)
        {
            vp_FPSCamera cam = GameManager.GetVpFPSCamera();
            if (cam == null) yield break;

            float riseDur = Mathf.Max(0.001f, Settings.Instance.RecoilRiseTime);
            float recFrac = Mathf.Clamp01(Settings.Instance.RecoilRecoveryFraction);
            float recDur = recFrac > 0f ? Mathf.Max(0.001f, Settings.Instance.RecoilRecoveryTime) : 0f;
            float total = riseDur + recDur;

            float elapsed = 0f;
            float pitchApplied = 0f;
            float yawApplied = 0f;

            while (elapsed < total)
            {
                elapsed += Time.deltaTime;

                float shape;
                if (elapsed <= riseDur)
                {
                    float u = Mathf.Clamp01(elapsed / riseDur);
                    shape = 1f - (1f - u) * (1f - u);
                }
                else if (recDur > 0f)
                {
                    float v = Mathf.Clamp01((elapsed - riseDur) / recDur);
                    float s = v * v * (3f - 2f * v);
                    shape = 1f - recFrac * s;
                }
                else
                {
                    shape = 1f;
                }

                float pitchTarget = pitchTotal * shape;
                cam.m_Pitch += pitchTarget - pitchApplied;
                pitchApplied = pitchTarget;

                float yawTarget = yawTotal * shape;
                cam.AddToYaw(yawTarget - yawApplied);
                yawApplied = yawTarget;

                yield return null;
            }
        }
    }
}
