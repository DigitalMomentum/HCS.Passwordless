using Umbraco.Cms.Infrastructure.Migrations;

namespace HCS.Umbraco.Passwordless.WebAuthn.Migrations;

internal sealed class AddHasEverIncrementedCounter : MigrationBase
{
    public AddHasEverIncrementedCounter(IMigrationContext context) : base(context) { }

    protected override void Migrate()
    {
        if (!ColumnExists("Passwordless_MemberCredentials", "HasEverIncrementedCounter"))
        {
            Alter.Table("Passwordless_MemberCredentials")
                .AddColumn("HasEverIncrementedCounter").AsBoolean().NotNullable().WithDefaultValue(false)
                .Do();
        }
    }
}
