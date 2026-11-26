CREATE TABLE [Permissions] (
    [Id] int NOT NULL IDENTITY,
    [Controller] nvarchar(100) NOT NULL,
    [Action] nvarchar(100) NOT NULL,
    [DisplayName] nvarchar(200) NOT NULL,
    CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id]),
    CONSTRAINT [UX_Permissions_Controller_Action] UNIQUE ([Controller], [Action])
);
GO
CREATE TABLE [RolePermissions] (
    [RoleId] nvarchar(450) NOT NULL,
    [PermissionId] int NOT NULL,
    CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([RoleId], [PermissionId]),
    CONSTRAINT [FK_RolePermissions_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RolePermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE
);
GO
CREATE INDEX [IX_RolePermissions_PermissionId] ON [RolePermissions] ([PermissionId]);
GO
