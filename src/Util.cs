﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;

namespace CustomCampaign
{
    public static class Util
    {
        public static void Popup(string text)
        {
            MelonLogger.Msg(text);
            KataConfig.I.CreateDebugText(text, new Vector3(0f, -1f, 5f), 5f, null, false, 0.2f);
        }
    }
}
