﻿/**
* Copyright 2015 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/

using FullSerializer;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Utilities;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class ExampleGetToken : MonoBehaviour
{
    private string _username;
    private string _password;
    private string _url;
    private string _workspaceId;
    private fsSerializer _serializer = new fsSerializer();

    private AuthenticationToken _authenticationToken;

    void Start ()
    {
        LogSystem.InstallDefaultReactors();

        VcapCredentials vcapCredentials = new VcapCredentials();
        fsData data = null;

        //  Get credentials from a credential file defined in environmental variables in the VCAP_SERVICES format. 
        //  See https://www.ibm.com/watson/developercloud/doc/common/getting-started-variables.html.
        var environmentalVariable = Environment.GetEnvironmentVariable("VCAP_SERVICES");
        var fileContent = File.ReadAllText(environmentalVariable);

        //  Add in a parent object because Unity does not like to deserialize root level collection types.
        fileContent = Utility.AddTopLevelObjectToJson(fileContent, "VCAP_SERVICES");

        //  Convert json to fsResult
        fsResult r = fsJsonParser.Parse(fileContent, out data);
        if (!r.Succeeded)
            throw new WatsonException(r.FormattedMessages);

        //  Convert fsResult to VcapCredentials
        object obj = vcapCredentials;
        r = _serializer.TryDeserialize(data, obj.GetType(), ref obj);
        if (!r.Succeeded)
            throw new WatsonException(r.FormattedMessages);

        //  Set credentials from imported credntials
        Credential credential = vcapCredentials.VCAP_SERVICES["conversation"][0].Credentials;
        _username = credential.Username.ToString();
        _password = credential.Password.ToString();
        _url = credential.Url.ToString();

        //  Get token
        if (!Utility.GetToken(OnGetToken, _url, _username, _password))
            Log.Debug("ExampleGetToken", "Failed to get token.");

        //  Check time remaining after x seconds.
        Runnable.Run(GetTokenTimeRemaining(90f));
    }

    private void OnGetToken(AuthenticationToken authenticationToken, string customData)
    {
        _authenticationToken = authenticationToken;
        Log.Debug("ExampleGetToken", "created: {0} | time to expiration: {1} minutes | token: {2}", _authenticationToken.Created, _authenticationToken.TimeUntilExpiration, _authenticationToken.Token);
    }

    private IEnumerator GetTokenTimeRemaining(float time)
    {
        yield return new WaitForSeconds(time);
        Log.Debug("ExampleGetToken", "created: {0} | time to expiration: {1} minutes | token: {2}", _authenticationToken.Created, _authenticationToken.TimeUntilExpiration, _authenticationToken.Token);
    }
}
