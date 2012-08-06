﻿namespace NSoft.NFramework.Data.Mappers {
    /// <summary>
    /// Fluent ADO.NET을 위해, DB Column 명과 Class의 속성명을 매핑시키는 클래스에 대한 인터페이스
    /// </summary>
    public interface INameMapper {
        /// <summary>
        /// 컬럼명을 속성명으로 맵핑시킨다.
        /// </summary>
        /// <param name="columnName">컬럼명</param>
        /// <returns>컬럼명에 매칭되는 속성명</returns>
        string MapToPropertyName(string columnName);
    }
}