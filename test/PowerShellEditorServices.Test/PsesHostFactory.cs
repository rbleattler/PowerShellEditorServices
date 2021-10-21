﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.PowerShell.EditorServices.Hosting;
using Microsoft.PowerShell.EditorServices.Services.PowerShell.Host;
using Microsoft.PowerShell.EditorServices.Test.Shared;
using Microsoft.PowerShell.EditorServices.Utility;

namespace Microsoft.PowerShell.EditorServices.Test
{
    internal static class PsesHostFactory
    {
        // NOTE: These paths are arbitrarily chosen just to verify that the profile paths
        //       can be set to whatever they need to be for the given host.

        public static readonly ProfilePathInfo TestProfilePaths =
            new ProfilePathInfo(
                    Path.GetFullPath(
                        TestUtilities.NormalizePath("../../../../PowerShellEditorServices.Test.Shared/Profile/Test.PowerShellEditorServices_profile.ps1")),
                    Path.GetFullPath(
                        TestUtilities.NormalizePath("../../../../PowerShellEditorServices.Test.Shared/Profile/ProfileTest.ps1")),
                    Path.GetFullPath(
                        TestUtilities.NormalizePath("../../../../PowerShellEditorServices.Test.Shared/Test.PowerShellEditorServices_profile.ps1")),
                    Path.GetFullPath(
                        TestUtilities.NormalizePath("../../../../PowerShellEditorServices.Test.Shared/ProfileTest.ps1")));

        public static readonly string BundledModulePath = Path.GetFullPath(
            TestUtilities.NormalizePath("../../../../../module"));

        public static System.Management.Automation.Runspaces.Runspace InitialRunspace;

        public static PsesInternalHost Create(ILoggerFactory loggerFactory)
        {
            // We intentionally use `CreateDefault2()` as it loads `Microsoft.PowerShell.Core` only,
            // which is a more minimal and therefore safer state.
            var initialSessionState = InitialSessionState.CreateDefault2();

            // We set the process scope's execution policy (which is really the runspace's scope) to
            // `Bypass` so we can import our bundled modules. This is equivalent in scope to the CLI
            // argument `-ExecutionPolicy Bypass`, which (for instance) the extension passes. Thus
            // we emulate this behavior for consistency such that unit tests can pass in a similar
            // environment.
            if (VersionUtils.IsWindows)
            {
                initialSessionState.ExecutionPolicy = ExecutionPolicy.Bypass;
            }

            HostStartupInfo testHostDetails = new HostStartupInfo(
                "PowerShell Editor Services Test Host",
                "Test.PowerShellEditorServices",
                new Version("1.0.0"),
                psHost: null,
                TestProfilePaths,
                featureFlags: Array.Empty<string>(),
                additionalModules: Array.Empty<string>(),
                initialSessionState,
                logPath: null,
                (int)LogLevel.None,
                consoleReplEnabled: false,
                usesLegacyReadLine: false,
                bundledModulePath: BundledModulePath);

            var psesHost = new PsesInternalHost(loggerFactory, null, testHostDetails);

            psesHost.TryStartAsync(new HostStartOptions { LoadProfiles = true }, CancellationToken.None).GetAwaiter().GetResult();

            return psesHost;
        }
    }
}

