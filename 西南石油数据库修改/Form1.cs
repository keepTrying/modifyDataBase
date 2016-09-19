using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Configuration;

namespace 西南石油数据库修改
{
    public partial class Form1 : Form
    {
        private String str_sqlsever_conn;
        private SqlConnection sql_conn;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            str_sqlsever_conn = ConfigurationManager.AppSettings["str_sql_conn"];
            try
            {
                sql_conn = new SqlConnection(str_sqlsever_conn);
                sql_conn.Open();
            }
            catch
            {
                MessageBox.Show("连接sql_sever数据库失败");
                Environment.Exit(0);
            }

            string sql;
            SqlCommand cmd;
            SqlDataReader reader;
            sql = "UPDATE [职业病体检_结果信息_生化科] SET 体检医师 = '4001' WHERE 体检医师 = '张青龙'";
            cmd = new SqlCommand(sql, sql_conn);
            cmd.ExecuteNonQuery();

            sql = "SELECT distinct 系统编号 from [职业病体检_结果信息_生化科] WHERE 体检医师<> ''";
            cmd = new SqlCommand(sql, sql_conn);
            reader = cmd.ExecuteReader();
            List<String> nos = new List<string>();
            while (reader.Read())
            {
                nos.Add(reader.GetString(0));
            }
            reader.Close();
            
            for(int i=0;i<nos.Count;i++)
            {
                String no = nos[i];
                #region 更改科室状态
                string department_state = "";
                sql = String.Format("select 各科体检状态 from 职业病体检_体检基本信息表 where 系统编号='{0}'", no);
                cmd = new SqlCommand(sql, sql_conn);
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                   department_state = reader.GetString(0);
                }
                reader.Close();
                department_state = department_state.Substring(0, 16) + "2";
                sql = String.Format("update 职业病体检_体检基本信息表 set 各科体检状态='{1}' where 系统编号='{0}'", no,department_state);
                cmd = new SqlCommand(sql, sql_conn);
                cmd.ExecuteNonQuery();
                #endregion
                #region 提交科室结论
                String department_conclusion="";
                SqlDataAdapter adapter;
                sql = "SELECT [系统编号], [体检医师], [填写时间], [单项结论],b.名称 FROM dbo.[职业病体检_结果信息_生化科] a left join dbo.职业病体检_体检项目设置表 b on a.体检项目= b.编码 where 系统编号='"+no+"'";
                adapter = new SqlDataAdapter(sql, sql_conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                String time="";
                foreach(DataRow row in dt.Rows)
                {
                    time = row[2].ToString();
                    switch (row[3].ToString())
                    {
                        case "偏高":
                        case "偏低":
                        case "阳性":
                        case "弱阳性":
                            department_conclusion += row[4].ToString() + row[3].ToString() + ";";
                            break;
                        default:
                            break;
                         
                    }
                }
                department_conclusion=department_conclusion==""?"未见异常":department_conclusion;
                String sql2 = "select * from 职业病体检_科室结论表 where 系统编号='" + no+ "' and 科室='17'";
                SqlCommand cmd2 = new SqlCommand(sql2, sql_conn);
                SqlDataReader read2 = cmd2.ExecuteReader();
                if (!read2.HasRows)
                {
                    read2.Close();
                    sql2 = "insert 职业病体检_科室结论表(系统编号,科室,文字结论,医生编号,结论日期,修改起始时间) values('" + no + "','17','" + department_conclusion + "','4001','" + time + "','" +time + "')";
                    cmd2 = new SqlCommand(sql2, sql_conn);
                    cmd2.ExecuteNonQuery();
                }
                else
                {
                    read2.Close();
                    //sql2 = "delete from 职业病体检_科室结论表 where 系统编号='" + no + "' and 科室='17';insert 职业病体检_科室结论表(系统编号, 科室, 文字结论, 医生编号, 结论日期, 修改起始时间) values('" + no + "', '17', '" + department_conclusion + "', '4001', '" + time + "', '" + DateTime.Now.ToString() + "')";
                    //cmd2 = new SqlCommand(sql2, sql_conn);
                    //cmd2.ExecuteNonQuery();

                }
                #endregion

            }
            MessageBox.Show("修改成功");
            Environment.Exit(0);
        }
    }
}
