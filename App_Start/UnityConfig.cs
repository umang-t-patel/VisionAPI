﻿// Copyright(c) 2016 Google Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not
// use this file except in compliance with the License. You may obtain a copy of
// the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
// License for the specific language governing permissions and limitations under
// the License.

using GoogleCloudSamples.Models;
using GoogleCloudSamples.Services;
using Microsoft.Practices.Unity;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Runtime.Serialization;

namespace GoogleCloudSamples
{
    public class ConfigurationException : Exception, ISerializable
    {
        public ConfigurationException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// We can store the VisionAPI data in different places.  This flag tells us where we to store
    /// the VisionAPI data.
    /// </summary>
    public enum VisionAPIStoreFlag
    {
        MySql,
        SqlServer,
        Datastore
    }

    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public class UnityConfig
    {
        #region Unity Container

        private static readonly Lazy<IUnityContainer> s_container = new Lazy<IUnityContainer>(() =>
        {
            var container = new UnityContainer();
            RegisterTypes(container);
            return container;
        });

        /// <summary>
        /// Gets the configured Unity container.
        /// </summary>
        public static IUnityContainer GetConfiguredContainer()
        {
            return s_container.Value;
        }

        #endregion Unity Container

        /// <summary>
        /// Looks for variable in app settings.
        /// Throws an exception of the key is not in the configuration.
        /// </summary>
        public static string GetConfigVariable(string key)
        {
            string value = ConfigurationManager.AppSettings[key];
            if (value == null)
                throw new ConfigurationException($"You must set the configuration variable {key}.");
            return value;
        }

        public static string ProjectId => GetConfigVariable("GoogleCloudSamples:ProjectId");

        public static VisionAPIStoreFlag ChooseVisionAPIStoreFromConfig()
        {
            string visionapiStore = GetConfigVariable("GoogleCloudSamples:VisionAPIStore")?.ToLower();
            switch (visionapiStore)
            {
                case "datastore":
                    return VisionAPIStoreFlag.Datastore;

                case "mysql":
                    DbConfiguration.SetConfiguration(new MySql.Data.Entity.MySqlEFConfiguration());
                    return VisionAPIStoreFlag.MySql;

                case "sqlserver":
                    return VisionAPIStoreFlag.SqlServer;

                default:
                    throw new ConfigurationException(
                         "Set the configuration variable GoogleCloudSamples:VisionAPIStore " +
                         "to datastore, mysql or sqlserver.");
            }
        }

        /// <summary>Registers the type mappings with the Unity container.</summary>
        /// <param name="container">The unity container to configure.</param>
        public static void RegisterTypes(IUnityContainer container)
        {
            switch (ChooseVisionAPIStoreFromConfig())
            {
                case VisionAPIStoreFlag.Datastore:
                    container.RegisterInstance<IVisionAPIStore>(
                        new DatastoreVisionAPIStore(ProjectId));
                    break;

                case VisionAPIStoreFlag.MySql:
                    container.RegisterType<IVisionAPIStore, DbVisionAPIStore>();
                    break;

                case VisionAPIStoreFlag.SqlServer:
                    container.RegisterType<IVisionAPIStore, DbVisionAPIStore>();
                    break;
            }

            container.RegisterInstance<ImageUploader>(
                new ImageUploader(
                  GetConfigVariable("GoogleCloudSamples:BucketName")
                )
            );
        }
    }
}