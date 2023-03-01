﻿using System;
using System.Globalization;
using Limbo.Umbraco.Feedback.Models.Statuses;
using Newtonsoft.Json;
using Skybrud.Essentials.Strings.Extensions;
using Umbraco.Cms.Core.Services;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Models.Api {

    public class StatusApiModel {

        #region Properties

        [JsonProperty("alias")]
        public string Alias { get; }

        [JsonProperty("key")]
        public Guid Key { get; }

        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("active")]
        public bool IsActive { get; }

        #endregion

        #region Constructors

        public StatusApiModel(FeedbackStatus status, ILocalizedTextService localizedTextService, CultureInfo culture) {

            Alias = status.Alias;
            Key = status.Key;
            Name = status.Name;
            IsActive = status.IsActive;

            if (string.IsNullOrWhiteSpace(status.Name)) {
                Name += localizedTextService.Localize("feedback", $"status{Alias.ToPascalCase()}", culture);
            }

        }

        #endregion

    }

}