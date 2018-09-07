using System;
using System.Collections.Generic;

namespace Vcredit.NetSpider.Entity.DB
{
    #region Chsi_InfoEntity

    /// <summary>
    /// Chsi_InfoEntity object for NHibernate mapped table 'Chsi_Info'.
    /// </summary>
    public class Chsi_InfoEntity
    {
        public virtual int Id { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public virtual string Name { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>
        public virtual string IdentityCard { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public virtual string Sex { get; set; }
        /// <summary>
        /// 出生日期
        /// </summary>
        public virtual DateTime? BirthDate { get; set; }
        /// <summary>
        /// 民族
        /// </summary>
        public virtual string Race { get; set; }
        /// <summary>
        /// 手机
        /// </summary>
        public virtual string Phone { get; set; }
        /// <summary>
        /// 考生号
        /// </summary>
        public virtual string ExamineeNo { get; set; }
        /// <summary>
        /// 学号
        /// </summary>
        public virtual string StudentNo { get; set; }
        /// <summary>
        /// 院校名称
        /// </summary>
        public virtual string University { get; set; }
        /// <summary>
        /// 学院
        /// </summary>
        public virtual string College { get; set; }
        /// <summary>
        /// 系(所、函授站)
        /// </summary>
        public virtual string Department { get; set; }
        /// <summary>
        /// 专业名称
        /// </summary>
        public virtual string MajorName { get; set; }
        /// <summary>
        /// 班级
        /// </summary>
        public virtual string Class { get; set; }
        /// <summary>
        /// 层次
        /// </summary>
        public virtual string Degree { get; set; }
        /// <summary>
        /// 学制
        /// </summary>
        public virtual string Schoolinglength { get; set; }
        /// <summary>
        /// 学历类别
        /// </summary>
        public virtual string EducationType { get; set; }
        /// <summary>
        /// 学习形式
        /// </summary>
        public virtual string LearningMode { get; set; }
        /// <summary>
        /// 入学日期
        /// </summary>
        public virtual DateTime? EnrollmentDate { get; set; }
        /// <summary>
        /// 学籍状态
        /// </summary>
        public virtual string SchoolState { get; set; }
        /// <summary>
        /// 离校日期
        /// </summary>
        public virtual DateTime? LeavingDate { get; set; }
        /// <summary>
        /// 证书编号
        /// </summary>
        public virtual string CertificateNo { get; set; }
        /// <summary>
        /// 毕结业状态
        /// </summary>
        public virtual string GraduateState { get; set; }
        /// <summary>
        /// 院校所在地
        /// </summary>
        public virtual string UniversityLocation { get; set; }
        /// <summary>
        /// 请求令牌
        /// </summary>
        public virtual string Token { get; set; }
        ///// <summary>
        ///// 毕业照片
        ///// </summary>
        //public virtual byte[] GraduatePhoto { get; set; }
        ///// <summary>
        ///// 录取照片
        ///// </summary>
        //public virtual byte[] EnrollPhoto { get; set; }

        /// <summary>
        /// 毕业照片
        /// </summary>
        public virtual string GraduatePhoto { get; set; }
        /// <summary>
        /// 录取照片
        /// </summary>
        public virtual string EnrollPhoto { get; set; }

        private DateTime _CreateTime = DateTime.Now;
        public virtual DateTime CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }
        //public virtual Spd_LoginEntity Spd_Login { get; set; }
    }
    #endregion
}