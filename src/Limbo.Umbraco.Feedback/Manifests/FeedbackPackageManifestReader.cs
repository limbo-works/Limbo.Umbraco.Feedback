﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Infrastructure.Manifest;

using static Limbo.Umbraco.Feedback.FeedbackPackage;

namespace Limbo.Umbraco.Feedback.Manifests;

#pragma warning disable CS1591

public class FeedbackPackageManifestReader : IPackageManifestReader {

    public async Task<IEnumerable<PackageManifest>> ReadPackageManifestsAsync() {

        List<PackageManifest> temp = [
            new() {
                Id = Alias,
                Name = Name,
                AllowTelemetry = true,
                Version = InformationalVersion,
                Extensions = [
                    new {
                        type = "bundle",
                        alias = $"{Alias}.Bundle",
                        name = $"{Name}: Bundle",
                        js = $"/App_Plugins/Limbo.Umbraco.Feedback/limbo-feedback.js"
                    }
                ]
            }

        ];

        return await Task.FromResult(temp);

    }

}