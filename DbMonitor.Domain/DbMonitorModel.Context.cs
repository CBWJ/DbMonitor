﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace DbMonitor.Domain
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class DbMonitorEntities : DbContext
    {
        public DbMonitorEntities()
            : base("name=DbMonitorEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Authority> Authority { get; set; }
        public virtual DbSet<Module> Module { get; set; }
        public virtual DbSet<ModuleAuthority> ModuleAuthority { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<RoleAuthority> RoleAuthority { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserRole> UserRole { get; set; }
        public virtual DbSet<v_RoleAuth> v_RoleAuth { get; set; }
        public virtual DbSet<SessionConnection> SessionConnection { get; set; }
    }
}
