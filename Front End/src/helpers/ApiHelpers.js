//import React from 'react';
import axios from 'axios';
import Config from './Config';
import { GetTokenForHeader, GetUserIdForHeader } from './CommonHelper';





export const MakeApiCallAsync = async (endPointName, methodSubURL, param, headers, methodType, loading = true) => {

    try {


        //--Add token in header
        if (headers !== null && headers !== undefined && !headers.hasOwnProperty('Token')) {
            let Token = await GetTokenForHeader();
            headers["Token"] = Token ?? "";
        }

        //--Add user id in header
        if (headers !== null && headers !== undefined && !headers.hasOwnProperty('UserID')) {

            let UserID = await GetUserIdForHeader();
            headers["UserID"] = UserID ?? "";
        }


        const URL = Config['ADMIN_BASE_URL'] + (methodSubURL === null || methodSubURL === undefined ? Config['DYNAMIC_METHOD_SUB_URL'] : methodSubURL) + endPointName;
        methodType = methodType ?? "POST";

        if (methodType === 'POST') {

            const response = await axios.post(URL, param, {
                headers: headers
            });

            return response;

        } else if (methodType === 'GET') {
            const response = await axios.get(URL, {
                headers: headers,
                param: param
            });

            return response;

        }

    } catch (error) {

        return error;
    }

}


export const MakeApiCallSynchronous = (endPointName, methodSubURL, param, headers, methodType, loading = true) => {


    const URL = Config['ADMIN_BASE_URL'] + (methodSubURL === null || methodSubURL === undefined ? Config['DYNAMIC_METHOD_SUB_URL'] : methodSubURL) + endPointName;

    methodType = methodType ?? "POST";


    if (methodType === 'POST') {
        try {
            const response = axios.post(URL, param, { headers: headers });

            return response;
        }
        catch (error) {

            return error;
        }
    }
    else if (methodType === 'GET') {

        try {
            const response = axios.get(URL, { headers: headers, param: param });

            return response;
        }
        catch (error) {

            return error;
        }
    }
}


