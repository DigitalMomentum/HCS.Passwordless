using Umbraco.Cms.Infrastructure.Migrations;

namespace HCS.Passwordless.WebAuthn.Migrations;

internal sealed class PasswordlessMigrationPlan : MigrationPlan
{
    public const string PlanName = "HCS.Passwordless";

    public PasswordlessMigrationPlan() : base(PlanName)
    {
        From(string.Empty)
            .To<AddPasswordlessMemberCredentials>("v1.0.0")
            .To<AddHasEverIncrementedCounter>("v1.1.0");
    }
}
