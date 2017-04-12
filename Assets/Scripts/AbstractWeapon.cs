using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractWeapon : MonoBehaviour
{
    public abstract float roundMaxDamage          { get; }
    public abstract float roundMinDamage          { get; }
    public abstract float roundMaxPrecisionDamage { get; }
    public abstract float roundMinPrecisionDamage { get; }
    public abstract float roundsPerSecond         { get; }
    public abstract int   magazineSize            { get; }
    public abstract float reloadSpeed             { get; }

    public abstract bool isAutomatic    { get; }
    public abstract float fireThreshold { get; }

    public abstract float hipMinRange         { get; }
    public abstract float hipMaxRange         { get; }
    public abstract float hipAccuracy         { get; }
    public abstract float hipRecoil           { get; }
    public abstract float hipAccuracyRecovery { get; }

    public abstract float adsMinRange         { get; }
    public abstract float adsMaxRange         { get; }
    public abstract float adsAccuracy         { get; }
    public abstract float adsRecoil           { get; }
    public abstract float adsAccuracyRecovery { get; }

    public abstract string slot { get; }
    public abstract string type { get; }
    public abstract int ammunitionCapacity { get; }
    public abstract float equipTime { get; }
    public abstract float adsTime { get; }
}
