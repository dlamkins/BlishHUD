﻿namespace Blish_HUD.Contexts {

    public class Gw2ClientContext : Context {

        /// <summary>
        /// The type of the client currently running.
        /// </summary>
        public enum ClientType {
            /// <summary>
            /// The client type could not be determined.
            /// </summary>
            Unknown,

            /// <summary>
            /// The client is the standard US/EU varient of the client.
            /// </summary>
            Standard,

            /// <summary>
            /// The client is the Chinese varient of the client.
            /// </summary>
            Chinese
        }

        /// <inheritdoc />
        protected override void Load() {
            this.ConfirmReady();
        }

        #region Specific Checks

        private bool IsStandardClientType(int currentBuildId, out ContextAvailability contextAvailability) {
            contextAvailability = GameService.Contexts.GetContext<CdnInfoContext>().TryGetStandardCdnInfo(out var standardCdnContextResult);

            if (contextAvailability == ContextAvailability.Available) {
                int standardBuildId = standardCdnContextResult.Value.BuildId;

                if (currentBuildId == standardBuildId) {
                    return true;
                }
            }

            return false;
        }

        private bool IsChineseClientType(int currentBuildId, out ContextAvailability contextAvailability) {
            contextAvailability = GameService.Contexts.GetContext<CdnInfoContext>().TryGetChineseCdnInfo(out var chineseCdnContextResult);

            if (contextAvailability == ContextAvailability.Available) {
                int chineseBuildId = chineseCdnContextResult.Value.BuildId;

                if (currentBuildId == chineseBuildId) {
                    return true;
                }
            }

            return false;
        }

        #endregion

        /// <summary>
        /// [DEPENDS ON: CdnInfoContext, Mumble Link API]
        /// If <see cref="ContextAvailability.Available"/>, returns if the client is the
        /// <see cref="ClientType.Standard"/> client or the <see cref="ClientType.Chinese"/> client.
        /// </summary>
        public ContextAvailability TryGetClientType(out ContextResult<ClientType> contextResult) {
            int currentBuildId;

            if (GameService.Gw2Mumble.Available) {
                currentBuildId = GameService.Gw2Mumble.BuildId;
            } else {
                contextResult = new ContextResult<ClientType>(ClientType.Unknown, "The Guild Wars 2 Mumble Link API was not available.");
                return ContextAvailability.Unavailable;
            }

            if (IsStandardClientType(currentBuildId, out var standardCdnStatus)) {
                contextResult = new ContextResult<ClientType>(ClientType.Standard, true);
                return ContextAvailability.Available;
            }

            if (IsChineseClientType(currentBuildId, out var chineseCdnStatus)) {
                contextResult = new ContextResult<ClientType>(ClientType.Chinese, true);
                return ContextAvailability.Available;
            }

            contextResult = new ContextResult<ClientType>(ClientType.Unknown, "The build ID reported by the Mumble Link API did not match the standard or the Chinese client build IDs provided by the CDN.");
            return ContextAvailability.Failed;
        }

    }
}