﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using GameJSON;
using JsonFx.Json;
using Debug = UnityEngine.Debug;

public class MyAPI
{
    private static string API_USER_LOGIN = "user/login";
    private static string API_FLOOR_COMPLETE = "floor/complete";

    public static API.Result _startURLRequest(string path, Dictionary<string, object> param, Action<URLRequest> onSuccess, Action<URLRequest> onFailed, API.Mode mode = API.Mode.NORMAL, bool isDataRequest = false)
    {
        Action<URLRequest> successHook = request =>
            {
                onSuccess(request);

                if (path.Equals(API_USER_LOGIN))
                {
                    ViewController.SwitchView(ViewIndex.WORLDMAP_WORLD_MAP, null, null);
                }
            };

        Action<URLRequest> failedHook = request =>
            {
                onFailed(request);
            };

        return (API.Result)API._startURLRequest(path, param, successHook, failedHook, mode, isDataRequest);
    }
}