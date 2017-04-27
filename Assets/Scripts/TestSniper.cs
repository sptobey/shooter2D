using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSniper : AbstractWeapon
{
    public override float roundMaxDamage { get { return 150.0f; } }
    public override float roundMinDamage { get { return 150.0f; } }
    public override float roundMaxPrecisionDamage { get { return 400.0f; } }
    public override float roundMinPrecisionDamage { get { return 200.0f; } }
    public override float roundsPerSecond { get { return 0.75f; } }
    public override int magazineSize { get { return 4; } }
    public override float reloadSpeed { get { return 2.5f; } }

    public override bool isAutomatic { get { return false; } }
    public override float fireThreshold { get { return 0.01f; } }

    public override float hipMinRange { get { return 2.0f; } }
    public override float hipMaxRange { get { return 5.0f; } }
    public override float hipAccuracy { get { return 60.0f; } }
    public override float hipRecoil { get { return 20.0f; } }
    public override float hipAccuracyRecovery { get { return 5.0f; } }

    public override float adsMinRange { get { return 15.0f; } }
    public override float adsMaxRange { get { return 45.0f; } }
    public override float adsAccuracy { get { return 0.5f; } }
    public override float adsRecoil { get { return 2.0f; } }
    public override float adsAccuracyRecovery { get { return 2.0f; } }

    public override string slot { get { return "Secondary"; } }
    public override string type { get { return "Sniper"; } }
    public override int ammunitionCapacity { get { return 25; } }
    public override float equipTime { get { return 0.35f; } }
    public override float adsTime { get { return 0.75f; } }
}
