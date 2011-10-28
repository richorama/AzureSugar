﻿#region Copyright (c) 2011 two10 degrees
//
// (C) Copyright 2011 two10 degrees
//      All rights reserved.
//
// This software is provided "as is" without warranty of any kind,
// express or implied, including but not limited to warranties as to
// quality and fitness for a particular purpose. Active Web Solutions Ltd
// does not support the Software, nor does it warrant that the Software
// will meet your requirements or that the operation of the Software will
// be uninterrupted or error free or that any defects will be
// corrected. Nothing in this statement is intended to limit or exclude
// any liability for personal injury or death caused by the negligence of
// Active Web Solutions Ltd, its employees, contractors or agents.
//
#endregion

namespace Two10.AzureSugar
{
    using System;
    using System.Text.RegularExpressions;
    using Microsoft.WindowsAzure.StorageClient;
    using Microsoft.WindowsAzure.StorageClient.Protocol;

    public static class AzureSugarBlobExtensions
    {
        public static CloudBlob GetBlobReference(this CloudBlobClient client, string containerName, string blobName)
        {
            return client.GetContainerReference(containerName).GetBlobReference(blobName);
        }

        public static bool CheckContainerName(this CloudBlobClient client, string containerName)
        {
            if (containerName == "$root")
            {
                return true;
            }

            return (Regex.IsMatch(containerName, @"(^([a-z]|\d))((-([a-z]|\d)|([a-z]|\d))+)$") && (3 <= containerName.Length) && (containerName.Length <= 63));
        }

        public static string AcquireLease(this CloudBlob blob)
        {
            var creds = blob.ServiceClient.Credentials;
            var transformedUri = new Uri(creds.TransformUri(blob.Uri.ToString()));
            var req = BlobRequest.Lease(transformedUri, 60, LeaseAction.Acquire, null);
            blob.ServiceClient.Credentials.SignRequest(req);
            using (var response = req.GetResponse())
            {
                return response.Headers["x-ms-lease-id"];
            }
        }

        public static void ReleaseLease(this CloudBlob blob, string leaseId)
        {
            var creds = blob.ServiceClient.Credentials;
            var transformedUri = new Uri(creds.TransformUri(blob.Uri.ToString()));
            var req = BlobRequest.Lease(transformedUri, 0, LeaseAction.Release, leaseId);
            blob.ServiceClient.Credentials.SignRequest(req);
            using (var response = req.GetResponse())
            {
            }
        }


        public static bool Exists(this CloudBlob blob)
        {
            try
            {
                blob.FetchAttributes();
                return true;
            }
            catch (StorageClientException e)
            {
                if (e.ErrorCode == StorageErrorCode.ResourceNotFound)
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
