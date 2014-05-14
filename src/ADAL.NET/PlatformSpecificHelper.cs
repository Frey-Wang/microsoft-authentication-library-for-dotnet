﻿//----------------------------------------------------------------------
// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// Apache License 2.0
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//----------------------------------------------------------------------

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    internal static class PlatformSpecificHelper
    {
        public static string GetProductName()
        {
            return ".NET";
        }

        public static string GetEnvironmentVariable(string variable)
        {
            string value = Environment.GetEnvironmentVariable(variable);
            return !string.IsNullOrWhiteSpace(value) ? value : null;
        }

        public static AuthenticationResult ProcessServiceError(string error, string errorDescription)
        {
            throw new AdalServiceException(error, errorDescription);
        }

        public static string PlatformSpecificToLower(this string input)
        {
            return input.ToLower(CultureInfo.InvariantCulture);
        }

        public static string GetUserPrincipalName()
        {
            string userId = System.DirectoryServices.AccountManagement.UserPrincipal.Current.UserPrincipalName;

            // On some machines, UserPrincipalName returns null
            if (string.IsNullOrWhiteSpace(userId))
            {
                const int NameUserPrincipal = 8;
                uint userNameSize = 0;
                GetUserNameEx(NameUserPrincipal, null, ref userNameSize);
                StringBuilder sb = new StringBuilder((int)userNameSize);
                GetUserNameEx(NameUserPrincipal, sb, ref userNameSize);            
            }

            return userId;
        }

        internal static string CreateSha256Hash(string input)
        {
            SHA256 sha256 = SHA256Managed.Create();
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] inputBytes = encoding.GetBytes(input);
            byte[] hashBytes = sha256.ComputeHash(inputBytes);
            string hash = Convert.ToBase64String(hashBytes);
            return hash;
        }

        [DllImport("secur32.dll", CharSet = CharSet.Auto)]
        private static extern int GetUserNameEx(int nameFormat, StringBuilder userName, ref uint userNameSize);
    }
}
