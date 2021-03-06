﻿using Bridge.Html5;
using System;

namespace ExpressCraft
{
    public class ExternalPlugin
    {
        public string SourceUrl;
        public bool SetupCompleted = false;
        public bool InLoad = false;

        public Action OnReady = null;

        public ExternalPlugin(string sourceUrl)
        {
            SourceUrl = sourceUrl;
        }

        public void Setup(bool async = false, bool defer = false)
        {
            if(!SetupCompleted)
            {
                if(InLoad) return;
                InLoad = true;
                var script = new HTMLScriptElement()
                {
                    OnLoad = (ele) =>
                    {
                        SetupCompleted = true;
                        InLoad = false;
                        if(OnReady != null)
                            OnReady();
                    },
                    Src = SourceUrl
                };
                if(async)
                    script.Async = async;
                if(defer)
                    script.Defer = defer;
                Document.Head.AppendChild(script);
            }
        }

        public void UsageCheck()
        {
            if(!SetupCompleted)
                throw new Exception("'" + SourceUrl + "' requires to be setup!");
            if(InLoad)
                throw new Exception("'" + SourceUrl + "' is currently loading, Please try again in a few seconds!");
        }
    }
}