﻿using FluentNHibernate.Mapping;
using NSoft.NFramework.TimePeriods;

namespace NSoft.NFramework.Data.NHibernateEx.Domain.Fluents.Timesheets {
    public class TimeSheetBaseMap : ClassMap<TimeSheetBase> {
        public TimeSheetBaseMap() {
            Table("ProjectTimeSheet");
            DynamicInsert();
            DynamicUpdate();
            Proxy<ITimeSheet>();

            DiscriminateSubClassesOnColumn("TimeSheetKind");

            Id(x => x.Id).Column("TimeSheetId").GeneratedBy.Assigned();

            Map(x => x.StartTime);
            Map(x => x.EndTime);
            Map(x => x.PeriodKind).CustomType<PeriodKind>();

            Map(x => x.PlanWeightValue);
            Map(x => x.ActualWeightValue);

            Map(x => x.PlanProgressValue);
            Map(x => x.ActualProgressValue);

            Map(x => x.PlanWorkValue);
            Map(x => x.ActualWorkValue);

            Map(x => x.PlanCostValue);
            Map(x => x.ActualCostValue);

            Map(x => x.IsComplete);
            Map(x => x.IsClosed);

            Map(x => x.Creator);
            Map(x => x.LastUpdator);

            Map(x => x.CreateDate);
            Map(x => x.UpdateTimestamp).CustomType("Timestamp");
        }
    }
}