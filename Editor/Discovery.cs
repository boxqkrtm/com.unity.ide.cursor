/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Unity Technologies.
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using System.IO;

namespace Microsoft.Unity.VisualStudio.Editor
{
	internal static class Discovery
	{
		public static IEnumerable<IVisualStudioInstallation> GetVisualStudioInstallations()
		{
			foreach (var installation in VisualStudioCursorInstallation.GetVisualStudioInstallations())
				yield return installation;
			foreach (var installation in VisualStudioCodiumInstallation.GetVisualStudioInstallations())
				yield return installation;
		}

		public static bool TryDiscoverInstallation(string editorPath, out IVisualStudioInstallation installation)
		{
			try
			{
				if (VisualStudioCursorInstallation.TryDiscoverInstallation(editorPath, out installation))
					return true;
				if (VisualStudioCodiumInstallation.TryDiscoverInstallation(editorPath, out installation))
					return true;
			}
			catch (IOException)
			{
				installation = null;
			}

			return false;
		}

		public static void Initialize()
		{
            VisualStudioCursorInstallation.Initialize();
            VisualStudioCodiumInstallation.Initialize();
		}
	}
}
