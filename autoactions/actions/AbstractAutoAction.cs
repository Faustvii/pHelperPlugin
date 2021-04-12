﻿namespace Turbo.plugins.patrick.autoactions.actions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Newtonsoft.Json;
    using parameters;
    using Plugins;
    using Plugins.Patrick.util;

    public abstract class AbstractAutoAction
    {
        public static readonly List<Type> AutoActionTypes = Misc.GetAllSubTypesFromType(typeof(AbstractAutoAction));

        public bool active { get; set; }
        
        [JsonIgnore] public string action { get { return GetType().Name; } }

        [JsonIgnore] public string attributes { get { return GetAttributes(); } }
        
        [JsonIgnore] 
        [Browsable(false)]
        public virtual long minimumExecutionDelta
        {
            get
            {
                return 1000;
            }
        }

        [JsonIgnore] 
        [Browsable(false)]
        public virtual string tooltip
        {
            get
            {
                return "No tooltip available for this auto action!";
            }
        }

        public abstract string GetAttributes();

        public abstract List<AbstractParameter> GetParameters();

        public abstract bool Applicable(IController hud);

        public abstract void Invoke(IController hud);
    }
}