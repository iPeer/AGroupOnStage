using AGroupOnStage.Logging;
using AGroupOnStage.Main;
using PreFlightTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSPEngineersReport = EngineersReport;

namespace AGroupOnStage.EngineersReport
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class EngineersReportInitialiser : MonoBehaviour 
    {

        private List<IDesignConcern> designConcerns;
        private bool hasAddedConcerns = false;

        public void Awake()
        {
            Log("Awake");
        }

        public void Start()
        {
            Log("Start");
            if (!AGOSMain.Settings.get<bool>("EnableEngineersReportHook"))
            {
                Log("Engineer's Report hook is disable by the user, aborting.");
                Destroy(this); // Free a miniscule amount of RAM!
                return;
            }
            Log("Registering GameEvents for Engineer's Report");
            GameEvents.onGUIEngineersReportReady.Add(AddConcerns);
            GameEvents.onGUIEngineersReportDestroy.Add(RemoveConcerns);
            designConcerns = new List<IDesignConcern>();
            Log("Generating list of AGOS Concerns...");
            designConcerns.AddRange(new IDesignConcern[] { new OutOfStageRangeConcern(), new InvalidPartReferenceConcern(), new NoGroupsConfiguredConcern() }.ToList());
            Log("{0} concern(s) will be added:", designConcerns.Count);
            foreach (IDesignConcern dc in designConcerns)
                Log("\t{0}: {1}", dc.ToString(), dc.GetSeverity().ToString());
            Log("Waiting for Engineer's Report 'ready' state...");

        }

        private void AddConcerns()
        {

            if (hasAddedConcerns)
            {
                LogWarning("Concerns are already registered!");
                return;
            }

            Log("Adding Concerns to Engineer's Report");
            foreach (IDesignConcern dc in designConcerns)
            {
                KSPEngineersReport.Instance.AddTest(dc);
                Log("Added '{0}'", dc.ToString());
            }
            hasAddedConcerns = true;

        }

        private void RemoveConcerns()
        {

            if (!hasAddedConcerns)
            {
                Log("Concerns are not registered - nothing to unregister.");
                return;
            }

            Log("Unregistering Concerns from Engineer's Report");
            foreach (IDesignConcern dc in designConcerns)
            {
                KSPEngineersReport.Instance.RemoveTest(dc);
                Log("Removed '{0}'", dc.ToString());
            }
            hasAddedConcerns = false;

        }

        private void LogWarning(string text, params object[] fillers)
        {
            Logger.LogWarning(String.Format("[EngineersReportInit]: " + text, fillers));
        }

        private void Log(string text, params object[] fillers)
        {
            Logger.Log(String.Format("[EngineersReportInit]: "+text, fillers));
        }

    }
}
