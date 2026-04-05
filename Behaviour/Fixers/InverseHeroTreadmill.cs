namespace Architect.Behaviour.Fixers;

// Decompiled with JetBrains decompiler
// Type: HeroTreadmill
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2E84A6F3-82D3-4AED-BF50-5670ED856678
// Assembly location: /Users/arunkapila/Library/Application Support/Steam/steamapps/common/Hollow Knight Silksong/Hollow Knight Silksong.app/Contents/Resources/Data/Managed/Assembly-CSharp.dll

using System;
using System.Collections;
using TeamCherry.SharedUtils;
using UnityEngine;

// resharper disable all
#pragma warning disable
#nullable disable
public class InverseHeroTreadmill : MonoBehaviour
{
  [Header("Treadmill")]
  [SerializeField]
  private ConveyorBelt conveyorBelt;
  [SerializeField]
  private Animator speedControlAnimator;
  [SerializeField]
  private CogRotationController cogRotationController;
  [SerializeField]
  private Transform vectorCurveAnimatorParent;
  [SerializeField]
  private ParticleSystemPool moveParticles;
  [SerializeField]
  private AudioSource runSource;
  [SerializeField]
  private AudioEvent startRunSound;
  [SerializeField]
  private AudioSource sprintSource;
  [SerializeField]
  private AudioEvent startSprintSound;
  [SerializeField]
  private AudioEvent endRunSound;
  [Space]
  [SerializeField]
  private MinMaxFloat speedRange;
  [SerializeField]
  private MinMaxFloat speedXRange;
  [SerializeField]
  private float speedLerpMultiplier;
  [Space]
  [SerializeField]
  private float heroReferenceSpeed;
  [Header("Gauge")]
  [SerializeField]
  private Transform needlePivot;
  [SerializeField]
  private PlayMakerFSM needleFsm;
  [SerializeField]
  private AudioSource needleRiseLoop;
  [Space]
  [SerializeField]
  private MinMaxFloat needleRange;
  [SerializeField]
  private float needleTarget;
  [SerializeField]
  private float needleRiseSpeed;
  [SerializeField]
  private float needleRiseLerpSpeed;
  [SerializeField]
  private float needleFallDelay;
  [SerializeField]
  private float needleFallSpeed;
  [SerializeField]
  private float needleFallLerpSpeed;
  [SerializeField]
  private JitterSelf needleJitter;
  [SerializeField]
  private float needleFpsLimit;
  private float oldSpeedMult = -1f;
  private HeroController capturedHero;
  private HeroController lastCapturedHero;
  private float targetSpeed;
  private float multiplier;
  private float currentSpeed;
  private bool wasMoving;
  private bool forceNeedleDrop;
  private double needleUpdateTime;
  private VectorCurveAnimator[] curveAnimators;
  private static readonly int _speedAnimatorParam = Animator.StringToHash("Speed");

  private void OnDrawGizmosSelected()
  {
    Gizmos.matrix = this.transform.localToWorldMatrix;
    Gizmos.DrawLine(new Vector3(this.speedXRange.Start, 2f, 0.0f), new Vector3(this.speedXRange.Start, 4f, 0.0f));
    Gizmos.DrawLine(new Vector3(this.speedXRange.End, 2f, 0.0f), new Vector3(this.speedXRange.End, 4f, 0.0f));
  }

  private void OnValidate()
  {
    if ((double) this.needleFpsLimit >= 0.0)
      return;
    this.needleFpsLimit = 0.0f;
  }

  private void Awake()
  {
    this.curveAnimators = (bool) (UnityEngine.Object) this.vectorCurveAnimatorParent ? this.vectorCurveAnimatorParent.GetComponentsInChildren<VectorCurveAnimator>() : new VectorCurveAnimator[0];
    this.conveyorBelt.CapturedHero += (Action<HeroController>) (hero =>
    {
      if ((bool) (UnityEngine.Object) this.capturedHero)
        this.capturedHero.BeforeApplyConveyorSpeed -= new Action<Vector2>(this.OnBeforeHeroConveyor);
      if ((bool) (UnityEngine.Object) hero)
      {
        hero.BeforeApplyConveyorSpeed += new Action<Vector2>(this.OnBeforeHeroConveyor);
        this.lastCapturedHero = hero;
      }
      else
        this.targetSpeed = 0.0f;
      this.capturedHero = hero;
    });
  }

  private void Start()
  {
    this.SetSpeedMultiplier(0.0f);
    this.StartCoroutine(this.NeedleControlRoutine());
  }

  public void OnDisable()
  {
    if (!(bool) (UnityEngine.Object) this.capturedHero)
      return;
    this.capturedHero.BeforeApplyConveyorSpeed -= new Action<Vector2>(this.OnBeforeHeroConveyor);
  }

  private void Update()
  {
    if ((double) Math.Abs(this.currentSpeed - this.targetSpeed) < 0.10000000149011612)
      this.currentSpeed = this.targetSpeed;
    this.currentSpeed = Mathf.Lerp(this.currentSpeed, this.targetSpeed, Time.deltaTime * this.speedLerpMultiplier);
    this.SetSpeedMultiplier(this.currentSpeed * this.multiplier);
    bool flag = (double) this.currentSpeed > 0.5;
    if (flag)
    {
      if (!this.wasMoving)
      {
        this.cogRotationController.ResetNextUpdateTime();
        this.moveParticles.PlayParticles();
        if (this.lastCapturedHero.cState.isSprinting)
          this.startSprintSound.SpawnAndPlayOneShot(this.sprintSource.transform.position);
        else
          this.startRunSound.SpawnAndPlayOneShot(this.runSource.transform.position);
      }
      if (this.lastCapturedHero.cState.isSprinting)
      {
        if (!this.sprintSource.isPlaying)
          this.sprintSource.Play();
        if (this.runSource.isPlaying)
          this.runSource.Stop();
      }
      else
      {
        if (this.sprintSource.isPlaying)
          this.sprintSource.Stop();
        if (!this.runSource.isPlaying)
          this.runSource.Play();
      }
    }
    else if (this.wasMoving)
    {
      this.moveParticles.StopParticles();
      this.endRunSound.SpawnAndPlayOneShot(this.runSource.transform.position);
      this.runSource.Stop();
      this.sprintSource.Stop();
    }
    this.wasMoving = flag;
  }

  private void OnBeforeHeroConveyor(Vector2 heroVelocity)
  {
    if (!this) return;
    if ((double) heroVelocity.x < 0.0)
    {
      float tbetween = this.speedXRange.GetTBetween(this.transform.InverseTransformPoint(this.capturedHero.transform.position).x);
      this.targetSpeed = (double) tbetween > 0.0 ? this.speedRange.GetLerpUnclampedValue(tbetween) : this.speedRange.Start;
      this.multiplier = heroVelocity.x / this.heroReferenceSpeed;
    }
    else
      this.targetSpeed = 0.0f;
  }

  private void SetSpeedMultiplier(float value)
  {
    if ((double) Math.Abs(value - this.oldSpeedMult) < 1.0 / 1000.0)
      return;
    this.oldSpeedMult = value;
    this.conveyorBelt.SpeedMultiplier = value;
    this.speedControlAnimator.SetFloat(HeroTreadmill._speedAnimatorParam, -value);
    this.cogRotationController.RotationMultiplier = -value;
    foreach (VectorCurveAnimator curveAnimator in this.curveAnimators)
      curveAnimator.SpeedMultiplier = value;
  }

  private IEnumerator NeedleControlRoutine()
  {
    this.SetNeedlePosition(0.0f);
    this.needleJitter.StopJitter();
    float needleT = 0.0f;
    float needleSpeed = 0.0f;
    float fallDelayElapsed = 0.0f;
    float targetT = this.needleRange.GetTBetween(this.needleTarget);
    bool wasAboveTarget = false;
    bool wasNeedleMoving = false;
    while (true)
    {
      if (this.wasMoving && !this.forceNeedleDrop)
      {
        needleSpeed = Mathf.Lerp(needleSpeed, this.needleRiseSpeed * this.multiplier, Time.deltaTime * this.needleRiseLerpSpeed);
        fallDelayElapsed = 0.0f;
      }
      else if ((double) needleT <= 0.0)
        needleSpeed = 0.0f;
      else if ((double) fallDelayElapsed >= (double) this.needleFallDelay || this.forceNeedleDrop)
      {
        needleSpeed = Mathf.Lerp(needleSpeed, -this.needleFallSpeed, Time.deltaTime * this.needleFallLerpSpeed);
      }
      else
      {
        needleSpeed = Mathf.Lerp(needleSpeed, 0.0f, Time.deltaTime * this.needleRiseLerpSpeed);
        fallDelayElapsed += Time.deltaTime;
      }
      bool flag1 = (double) Math.Abs(needleSpeed) > 0.009999999776482582;
      if (flag1)
      {
        if (!wasNeedleMoving)
        {
          this.needleJitter.StartJitter();
          this.needleRiseLoop.Play();
        }
      }
      else if (wasNeedleMoving)
      {
        this.needleJitter.StopJitter();
        this.needleRiseLoop.Stop();
      }
      needleT = Mathf.Clamp01(needleT + needleSpeed * Time.deltaTime);
      if ((double) this.needleFpsLimit > 0.0)
      {
        if (Time.timeAsDouble > this.needleUpdateTime)
        {
          this.needleUpdateTime = Time.timeAsDouble + 1.0 / (double) this.needleFpsLimit;
          this.SetNeedlePosition(needleT);
        }
      }
      else
        this.SetNeedlePosition(needleT);
      if ((double) needleT <= 0.009999999776482582)
        this.forceNeedleDrop = false;
      bool flag2 = (double) needleT >= (double) targetT;
      if (flag2)
      {
        if (!wasAboveTarget)
          this.needleFsm.SendEvent("NEEDLE ABOVE");
      }
      else if (wasAboveTarget)
        this.needleFsm.SendEvent("NEEDLE BELOW");
      wasAboveTarget = flag2;
      wasNeedleMoving = flag1;
      yield return (object) null;
    }
  }

  private void SetNeedlePosition(float value)
  {
    this.needlePivot.transform.SetLocalRotation2D(this.needleRange.GetLerpedValue(value));
  }

  public void DropNeedle() => this.forceNeedleDrop = true;
}
