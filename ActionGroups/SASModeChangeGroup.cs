using AGroupOnStage.Logging;
using AGroupOnStage.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AGroupOnStage.ActionGroups
{
    class SASModeChangeGroup : AGOSActionGroup
    {

        public override void fire()
        {
            this.fireOnVessel(FlightGlobals.fetch.activeVessel);
        }

        public override void fireOnVessel(Vessel v)
        {
            int sasMode = this.fireGroupID;
            VesselAutopilot.AutopilotMode mode = AGroupOnStage.Main.AGOSUtils.getSASModeForID(sasMode);
            if (!v.ActionGroups.groups[16]) // SAS disabled
            {

                v.ActionGroups.SetGroup(KSPActionGroup.SAS, true);

            }
            if (v.Autopilot.CanSetMode(mode))
            {
                // This is about as hacky as they come, really
                GameObject autopilotUI = GameObject.Find("AutopilotModes2");
                VesselAutopilotUI vesselAutopilotUI = autopilotUI.GetComponent<VesselAutopilotUI>();
                Logger.Log("Current vessel Autopilot mode: {0}", v.Autopilot.Mode);
                Logger.Log("Setting SAS mode to '{0}' on vessel '{1}' ({2}, {3})", mode.ToString(), v.vesselName, v.rootPart.flightID, v.id);
                if (!(AGOSMain.Instance.isGameGUIHidden && AGOSMain.Settings.get<bool>("SilenceWhenUIHidden")))
                    ScreenMessages.PostScreenMessage(String.Format("Vessel's autopilot has been set to '{0}'", mode), 5f, ScreenMessageStyle.UPPER_CENTER);
                vesselAutopilotUI.modeButtons[(int)v.Autopilot.Mode].SetState(KSP.UI.UIStateToggleButton.BtnState.False);
                vesselAutopilotUI.modeButtons[(int)mode].SetState(KSP.UI.UIStateToggleButton.BtnState.True);
                v.Autopilot.SetMode(mode);
                v.Autopilot.Update();
                Logger.Log("New vessel Autopilot mode: {0}", v.Autopilot.Mode);
            }
            else
            {
                Logger.LogWarning("Vessel '{1}' ({2}, {3}) cannot be set into SAS mode '{0}'", mode, v.vesselName, v.rootPart.flightID, v.id);
                if (!(AGOSMain.Instance.isGameGUIHidden && AGOSMain.Settings.get<bool>("SilenceWhenUIHidden")))
                    ScreenMessages.PostScreenMessage(String.Format("Vessel cannot be set to autopilot mode '{0}' because that mode is not available.", mode), 5f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        public override void fireOnVesselID(uint vID)
        {

            Vessel vessel = FlightGlobals.fetch.vessels.Find(v => v.rootPart.flightID == vID);

            if (vessel == null)
            {
                Logger.LogError("Tried to fire action group on vessel ID {0} but no such vessel ID exists", vID);
                return;
            }

            fireOnVessel(vessel);

        }

    }
}
