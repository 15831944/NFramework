﻿using System;

namespace NSoft.NFramework.Data.NHibernateEx.Domain.Mappings {
    [Serializable]
    public class JoinMaster : DataEntityBase<Int32> {
        public virtual string Name { get; set; }

        public virtual string NickName { get; set; }

        public virtual string Description { get; set; }

        public override int GetHashCode() {
            if(IsSaved)
                return base.GetHashCode();

            return HashTool.Compute(Name);
        }
    }
}