using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace HCS.Passwordless.WebAuthn.Migrations;

internal sealed class AddPasswordlessMemberCredentials : MigrationBase
{
    public AddPasswordlessMemberCredentials(IMigrationContext context) : base(context) { }

    protected override void Migrate()
    {
        if (TableExists("Passwordless_MemberCredentials")) return;

        Create.Table("Passwordless_MemberCredentials")
            .WithColumn("Id").AsGuid().PrimaryKey().WithDefaultValue(SystemMethods.NewGuid)
            .WithColumn("MemberKey").AsGuid().NotNullable().Indexed("IX_Passwordless_MemberCredentials_MemberKey")
            .WithColumn("CredentialId").AsBinary(1024).NotNullable()
            .WithColumn("PublicKey").AsBinary(int.MaxValue).NotNullable()
            .WithColumn("UserHandle").AsBinary(64).NotNullable()
            .WithColumn("SignatureCounter").AsInt64().NotNullable().WithDefaultValue(0)
            .WithColumn("CredType").AsString(32).NotNullable()
            .WithColumn("AaGuid").AsGuid().NotNullable().WithDefaultValue(Guid.Empty)
            .WithColumn("Transports").AsString(128).Nullable()
            .WithColumn("BackupEligible").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("BackupState").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("Nickname").AsString(64).Nullable()
            .WithColumn("CreatedUtc").AsDateTime().NotNullable()
            .WithColumn("LastUsedUtc").AsDateTime().Nullable()
            .WithColumn("AttestationFormat").AsString(32).Nullable()
            .WithColumn("HasEverIncrementedCounter").AsBoolean().NotNullable().WithDefaultValue(false)
            .Do();

        Create.Index("UX_Passwordless_MemberCredentials_CredentialId")
            .OnTable("Passwordless_MemberCredentials")
            .OnColumn("CredentialId")
            .Unique()
            .Do();
    }
}
