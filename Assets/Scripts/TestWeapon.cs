﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestWeapon : AbstractWeapon
{
    public override float roundMaxDamage          { get { return 50.0f; } }
    public override float roundMinDamage          { get { return 25.0f; } }
    public override float roundMaxPrecisionDamage { get { return 75.0f; } }
    public override float roundMinPrecisionDamage { get { return 75.0f; } }
    public override float roundsPerSecond         { get { return 5.0f; } }
    public override int   magazineSize            { get { return 8; } }
    public override float reloadSpeed             { get { return 2.5f; } }

    public override bool isAutomatic    { get { return true; } }
    public override float fireThreshold { get { return 0.01f; } }

    public override float hipMinRange         { get { return 5.0f; } }
    public override float hipMaxRange         { get { return 10.0f; } }
    public override float hipAccuracy         { get { return 20.0f; } }
    public override float hipRecoil           { get { return 5.0f; } }
    public override float hipAccuracyRecovery { get { return 1.0f; } }

    public override float adsMinRange         { get { return 10.0f; } }
    public override float adsMaxRange         { get { return 15.0f; } }
    public override float adsAccuracy         { get { return 5.0f; } }
    public override float adsRecoil           { get { return 2.5f; } }
    public override float adsAccuracyRecovery { get { return 2.0f; } }

    public override string slot { get { return "Primary"; } }
    public override string type { get { return "Hand Cannon"; } }
    public override int ammunitionCapacity { get { return 100;  } }
    public override float equipTime { get  { return 0.25f; } }
    public override float adsTime { get { return 0.5f; } }
}
