﻿using Bridge.Html5;
using Bridge.jQuery2;
using System;

namespace ExpressCraft
{
    public static class Network
    {
        private static AjaxOptions GetAjaxOptions(object JsonFile, bool Async = true)
        {
            return new AjaxOptions()
            {
                Async = Async,
                Url = Settings.NetworkURL,
                Cache = false,
                Data = JsonFile == null ? string.Empty : JSON.Stringify(JsonFile),
                DataType = "json",
                ContentType = "application/json",
                Type = "POST"
            };
        }

        public class MethodRequest
        {
            public string Method;
            public object[] Arguments;
            public string Interface;

            public MethodRequest(string interfaceName, string method, params object[] arguments)
            {
                Method = method;
                Arguments = arguments;
                Interface = interfaceName;
            }
        }

        public static void InvokeMethodUI(string interfaceName, string method, Action<object, string, jqXHR> Success = null, Action<jqXHR, string, string> Error = null, params object[] arguments)
        {
            PostJsonProgressForm(new MethodRequest(interfaceName, method, arguments), Success, Error);
        }

        public static void InvokeMethodUIControl(string interfaceName, string method, ProgressControl progressControl, Action<object, string, jqXHR> Success = null, Action<jqXHR, string, string> Error = null, params object[] arguments)
        {
            PostJsonProgressControl(new MethodRequest(interfaceName, method, arguments), progressControl, Success, Error);
        }

        public static void InvokeMethod(string interfaceName, string method, Action<object, string, jqXHR> Success = null, Action<jqXHR, string, string> Error = null, params object[] arguments)
        {
            PostJson(new MethodRequest(interfaceName, method, arguments), Success, Error);
        }

        public static void PostJson(object JsonFile, Action<object, string, jqXHR> Success = null, Action<jqXHR, string, string> Error = null, bool Async = true)
        {
            // lets convert the JsonFileObject to a string;
            var ajo = GetAjaxOptions(JsonFile, Async);
            ajo.Success = Success;
            ajo.Error = Error;

            jQuery.Ajax(ajo);
        }

        public static void PostJsonProgressControl(object JsonFile, ProgressControl progressControl, Action<object, string, jqXHR> Success = null, Action<jqXHR, string, string> Error = null, bool Async = true)
        {
            // lets convert the JsonFileObject to a string;
            var ajo = GetAjaxOptions(JsonFile, Async);
            ajo.Xhr = () =>
            {
                var xmlRequest = new XMLHttpRequest();
                xmlRequest.AddEventListener(EventType.Progress, (e) =>
                {
                    var pe = e as ProgressEvent;
                    if(progressControl == null)
                        return;
                    var pc = progressControl;

                    float Percent = 0;

                    if(pe.Loaded != 0 && pe.Total != 0)
                    {
                        Percent = ((float)pe.Loaded / (float)pe.Total) * 100.0f;
                    }
                    pc.internalProgressControl.Style.Width = Percent.ToString() + "%";
                });

                return xmlRequest;
            };
            ajo.Success = Success;
            ajo.Error = Error;

            jQuery.Ajax(ajo);
        }

        public static void PostJsonProgressForm(object JsonFile, Action<object, string, jqXHR> Success = null, Action<jqXHR, string, string> Error = null, bool Async = true)
        {
            // lets convert the JsonFileObject to a string;
            var npf = new NetworkProgressForm();

            var ajo = GetAjaxOptions(JsonFile, Async);
            ajo.Xhr = () =>
            {
                var xmlRequest = new XMLHttpRequest();
                xmlRequest.AddEventListener(EventType.Progress, (e) =>
                {
                    var pe = e as ProgressEvent;
                    if(npf == null || npf.progressControl == null)
                        return;
                    var pc = npf.progressControl;

                    float Percent = 0;

                    if(pe.Loaded != 0 && pe.Total != 0)
                    {
                        Percent = ((float)pe.Loaded / (float)pe.Total) * 100.0f;
                    }
                    pc.internalProgressControl.Style.Width = Percent.ToString() + "%";
                });

                return xmlRequest;
            };
            ajo.Success = (o, s, jq) =>
            {
                npf.DialogResult = DialogResultEnum.OK;
                Success(o, s, jq);
            };
            ajo.Error = (jq, s1, s2) =>
            {
                npf.DialogResult = DialogResultEnum.Cancel;
                Error(jq, s1, s2);
            };
            ajo.Complete = (jq, str) => { npf.Close(); };

            var ajr = jQuery.Ajax(ajo);

            npf.ShowDialog(new DialogResult(DialogResultEnum.Cancel, () =>
            {
                ajr.Abort();
            }));
        }

        public class NetworkProgressForm : Form
        {
            public ProgressControl progressControl;
            public SimpleDialogButton buttonCancel;

            public NetworkProgressForm(string _text = "Loading...")
            {
                this.Text = _text;
                this.Width = 400;
                this.Height = 200;

                progressControl = new ProgressControl();
                progressControl.SetBounds(50, 50, "(100% - 100px)", "23px");

                buttonCancel = new SimpleDialogButton(this, DialogResultEnum.Cancel) { Text = "Cancel" };
                buttonCancel.SetLocation("(100% - 78px)", "(100% - 26px)");
                buttonCancel.Content.TabIndex = 0;

                Body.AppendChildren(buttonCancel, progressControl);

                AllowSizeChange = false;
            }
        }
    }
}