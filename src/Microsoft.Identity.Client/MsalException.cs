﻿//----------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Identity.Client.Exceptions;
using Microsoft.Identity.Client.Utils;
using Microsoft.Identity.Json.Linq;

namespace Microsoft.Identity.Client
{
    /// <summary>
    /// Base exception type thrown when an error occurs during token acquisition.
    /// For more details, see https://aka.ms/msal-net-exceptions
    /// </summary>
    /// <remarks>Avoid throwing this exception. Instead throw the more specialized <see cref="MsalClientException"/>
    /// or <see cref="MsalServiceException"/>
    /// </remarks>
    public class MsalException : Exception
    {
        /// <summary>
        /// Unknown Error occured.
        /// <para>Mitigation</para> None. You might want to inform the end user.
        /// </summary>
        public const string UnknownError = "unknown_error";

        /// <summary>
        /// Initializes a new instance of the exception class.
        /// </summary>
        public MsalException()
            : base(MsalErrorMessage.Unknown)
        {
            ErrorCode = UnknownError;
        }

        /// <summary>
        /// Initializes a new instance of the exception class with a specified
        /// error code.
        /// </summary>
        /// <param name="errorCode">
        /// The error code returned by the service or generated by client. This is the code you can rely on
        /// for exception handling.
        /// </param>
        public MsalException(string errorCode)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the exception class with a specified
        /// error code and error message.
        /// </summary>
        /// <param name="errorCode">
        /// The error code returned by the service or generated by client. This is the code you can rely on
        /// for exception handling.
        /// </param>
        /// <param name="errorMessage">The error message that explains the reason for the exception.</param>
        public MsalException(string errorCode, string errorMessage)
            : base(errorMessage)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the exception class with a specified
        /// error code and a reference to the inner exception that is the cause of
        /// this exception.
        /// </summary>
        /// <param name="errorCode">
        /// The error code returned by the service or generated by client. This is the code you can rely on
        /// for exception handling.
        /// </param>
        /// <param name="errorMessage">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, or a null reference if no inner
        /// exception is specified.
        /// </param>
        public MsalException(string errorCode, string errorMessage, Exception innerException)
            : base(errorMessage, innerException)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Gets the protocol error code returned by the service or generated by client. This is the code you can rely on for
        /// exception handling. Values for this code are typically provided in constant strings in the derived exceptions types
        /// with explanations of mitigation.
        /// </summary>
        public string ErrorCode { get; private set; }

        /// <summary>
        /// Creates and returns a string representation of the current exception.
        /// </summary>
        /// <returns>A string representation of the current exception.</returns>
        public override string ToString()
        {
            return base.ToString() + string.Format(CultureInfo.InvariantCulture, "\n\tErrorCode: {0}", ErrorCode);
        }

        #region SERIALIZATION

        private const string ExceptionTypeKey = "type";
        private const string ErrorCodeKey = "error_code";
        private const string ErrorDescriptionKey = "error_description";

        internal virtual void PopulateJson(JObject jobj)
        {
            jobj[ExceptionTypeKey] = GetType().Name;
        }

        internal virtual void PopulateObjectFromJson(JObject jobj)
        {
        }

        /// <summary>
        /// Allows serialization of most values of the exception into JSON.
        /// </summary>
        /// <returns></returns>
        public string ToJsonString()
        {
            JObject jobj = new JObject();
            PopulateJson(jobj);
            return jobj.ToString();
        }

        private static readonly Lazy<Dictionary<string, Type>> s_typeNameToType = new Lazy<Dictionary<string, Type>>(() =>
        {
            return new Dictionary<string, Type>
            {
                { typeof(MsalException).Name, typeof(MsalException) },
                { typeof(MsalClientException).Name, typeof(MsalClientException) },
                { typeof(MsalServiceException).Name, typeof(MsalServiceException) },
                { typeof(MsalUiRequiredException).Name, typeof(MsalUiRequiredException) },
            };
        });

        /// <summary>
        /// Allows re-hydration of the MsalException (or one of its derived types) from JSON generated by ToJsonString().
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static MsalException FromJsonString(string json)
        {
            JObject jobj = JObject.Parse(json);
            string type = jobj.Value<string>(ExceptionTypeKey);

            if (s_typeNameToType.Value.TryGetValue(type, out Type exceptionType))
            {
                string errorCode = JsonUtils.GetExistingOrEmptyString(jobj, ErrorCodeKey);
                string errorMessage = JsonUtils.GetExistingOrEmptyString(jobj, ErrorDescriptionKey);

                MsalException ex = Activator.CreateInstance(exceptionType, errorCode, errorMessage) as MsalException;
                ex.PopulateObjectFromJson(jobj);
                return ex;
            }

            throw new MsalClientException(MsalError.JsonParseError,  MsalErrorMessage.MsalExceptionFailedToParse);
        }

        #endregion // SERIALIZATION
    }
}
