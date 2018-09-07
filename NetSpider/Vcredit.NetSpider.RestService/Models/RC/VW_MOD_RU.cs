using System;
using System.Text;
using System.Collections.Generic;
using System.Data;

namespace Vcredit.NetSpider.RestService.Models.RC
{
    [Serializable]
    public partial class VW_MOD_RU
    {
        #region Fields
        /// <summary>
        /// MOD_ID
        /// </summary>	
        protected int _mod_id;

        /// <summary>
        /// MOD_NM
        /// </summary>	
        protected string _mod_nm;

        /// <summary>
        /// RU_GP_ID
        /// </summary>	
        protected int? _ru_gp_id;

        /// <summary>
        /// RU_GP_TP
        /// </summary>	
        protected int? _ru_gp_tp;

        /// <summary>
        /// RU_GP_SEQ
        /// </summary>	
        protected int? _ru_gp_seq;

        /// <summary>
        /// RU_GP_CAL
        /// </summary>	
        protected int? _ru_gp_cal;

        /// <summary>
        /// RU_GP_OP
        /// </summary>	
        protected string _ru_gp_op;

        /// <summary>
        /// RU_GP_TH
        /// </summary>	
        protected decimal? _ru_gp_th;

        /// <summary>
        /// RU_EXP
        /// </summary>	
        protected string _ru_exp;

        /// <summary>
        /// RU_NM
        /// </summary>	
        protected string _ru_nm;

        /// <summary>
        /// RU_ID
        /// </summary>	
        protected int? _ru_id;

         /// <summary>
        /// OriginalRuID
        /// </summary>	
        protected int? _OriginalRuID;
        
        /// <summary>
        /// RUGP_REL_SEQ
        /// </summary>	
        protected int? _rugp_rel_seq;

        /// <summary>
        /// VB_COL
        /// </summary>
        protected string _vb_col;

         /// <summary>
        /// RU_VER_GP
        /// </summary>	
        protected int _ru_ver_gp;

        /// <summary>
        /// RU_DESC
        /// </summary>
        protected string _ru_desc;
        #endregion

        #region Ctor
        public VW_MOD_RU() { }
        #endregion

        #region Property
        /// <summary>
        /// MOD_ID
        /// </summary>		
        public int MOD_ID
        {
            get { return _mod_id; }
            set { _mod_id = value; }
        }

        /// <summary>
        /// MOD_NM
        /// </summary>		
        public string MOD_NM
        {
            get { return _mod_nm; }
            set { _mod_nm = value; }
        }

        /// <summary>
        /// RU_GP_ID
        /// </summary>		
        public int? RU_GP_ID
        {
            get { return _ru_gp_id; }
            set { _ru_gp_id = value; }
        }

        /// <summary>
        /// RU_GP_TP
        /// </summary>		
        public int? RU_GP_TP
        {
            get { return _ru_gp_tp; }
            set { _ru_gp_tp = value; }
        }

        /// <summary>
        /// RU_GP_SEQ
        /// </summary>		
        public int? RU_GP_SEQ
        {
            get { return _ru_gp_seq; }
            set { _ru_gp_seq = value; }
        }

        /// <summary>
        /// RU_GP_CAL
        /// </summary>		
        public int? RU_GP_CAL
        {
            get { return _ru_gp_cal; }
            set { _ru_gp_cal = value; }
        }

        /// <summary>
        /// RU_GP_OP
        /// </summary>		
        public string RU_GP_OP
        {
            get { return _ru_gp_op; }
            set { _ru_gp_op = value; }
        }

        /// <summary>
        /// RU_GP_TH
        /// </summary>		
        public decimal? RU_GP_TH
        {
            get { return _ru_gp_th; }
            set { _ru_gp_th = value; }
        }

        /// <summary>
        /// RU_EXP
        /// </summary>		
        public string RU_EXP
        {
            get { return _ru_exp; }
            set { _ru_exp = value; }
        }

        /// <summary>
        /// RU_NM
        /// </summary>		
        public string RU_NM
        {
            get { return _ru_nm; }
            set { _ru_nm = value; }
        }

        /// <summary>
        /// RU_ID
        /// </summary>		
        public int? RU_ID
        {
            get { return _ru_id; }
            set { _ru_id = value; }
        }

         /// <summary>
        /// OriginalRuID
        /// </summary>		
        public int? OriginalRuID
        {
            get { return _OriginalRuID; }
            set { _OriginalRuID = value; }
        }
        
        /// <summary>
        /// RUGP_REL_SEQ
        /// </summary>		
        public int? RUGP_REL_SEQ
        {
            get { return _rugp_rel_seq; }
            set { _rugp_rel_seq = value; }
        }

        /// <summary>
        /// VB_COL
        /// </summary>		
        public string VB_COL
        {
            get { return _vb_col; }
            set { _vb_col = value; }
        }

         /// <summary>
        /// RU_VER_GP
        /// </summary>		
        public int RU_VER_GP
        {
            get { return _ru_ver_gp; }
            set { _ru_ver_gp = value; }
        }

          /// <summary>
        /// RU_DESC
        /// </summary>		
        public string RU_DESC
        {
            get { return _ru_desc; }
            set { _ru_desc = value; }
        }

        
        
        #endregion
    }
}