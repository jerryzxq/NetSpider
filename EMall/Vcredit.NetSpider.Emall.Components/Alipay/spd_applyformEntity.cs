using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.DataAnnotations;

namespace Vcredit.NetSpider.Emall.Entity
{
    [Alias("spd_applyform")]
    public class spd_applyformEntity
    {

        public spd_applyformEntity() { }


        #region Attributes

        private int id;

        [AutoIncrement]
        /// <summary>
        /// 
        /// </summary>
        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        private int? applyId;


        /// <summary>
        /// 申请单ID
        /// </summary>
        public int? ApplyId
        {
            get { return applyId; }
            set { applyId = value; }
        }

        private string form_name;


        /// <summary>
        /// 表单name
        /// </summary>
        public string Form_name
        {
            get { return form_name; }
            set { form_name = value; }
        }

        private string form_value;


        /// <summary>
        /// 表单name
        /// </summary>
        public string Form_value
        {
            get { return form_value; }
            set { form_value = value; }
        }

        private DateTime? createtime;


        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? Createtime
        {
            get { return createtime; }
            set { createtime = value; }
        }
        #endregion

    }
}
