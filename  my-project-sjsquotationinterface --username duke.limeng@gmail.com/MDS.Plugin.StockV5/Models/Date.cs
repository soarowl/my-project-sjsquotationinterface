using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MDS.Plugin.SZQuotV5
{
    /// <summary>
    /// 表示日期
    /// </summary>
    public struct Date : IComparable<Date>
    {
        const string pattern = @"^((?:19|20)\d{2})(1[0-2]|0[1-9])(3[0-1]|0[1-9]|[1-2][0-9])$";

        private int _year;
        private int _month;
        private int _day;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <param name="day">日</param>
        public Date(int year, int month, int day)
        {
            this._year = year;
            this._month = month;
            this._day = day;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="date"></param>
        public Date(DateTime date)
        {
            this._year = date.Year;
            this._month = date.Month;
            this._day = date.Day;

        }


        /// <summary>
        /// 获取日期中“年”的部分
        /// </summary>
        public int Year { get { return _year; } }


        /// <summary>
        /// 获取日期中“月”的部分
        /// </summary>
        public int Month { get { return _month; } }

        /// <summary>
        /// 获取日期中“日”的部分
        /// </summary>
        public int Day { get { return _day; } }

        /// <summary>
        /// 比较日期是否相等
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public bool DateEquals(DateTime dateTime)
        {
            if (this.Year == dateTime.Year
                && this.Month == dateTime.Month
                && this.Day == dateTime.Day)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 重写GetHashCode方法
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        /// <summary>
        /// 以YMMDD的格式返回日期的int表示形式
        /// </summary>
        /// <returns></returns>
        public int ToYMMDD()
        {
            return Year * 10000 + Month * 100 + Day;
        }
        /// <summary>
        /// 将一个DateTime类型的对象转换为一个YMMDD格式的int
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static int ToYMMDD(DateTime datetime)
        {
            return new Date(datetime).ToYMMDD();
        }

        /// <summary>
        /// 转换为DateTime类型
        /// </summary>
        /// <returns></returns>
        public DateTime ToDateTime()
        {
            try
            {
                return new DateTime(this.Year, this.Month, this.Day);
            }
            catch
            {
                return default(DateTime);
            }
        }


        /// <summary>
        /// 将字符串解析为一个Date类型的值
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Date Parse(string str)
        {
            Date rtn;
            if (TryParse(str, out rtn))
                return rtn;
            else
            {
                throw new FormatException(string.Format("无法将字符串 {0} 解析为Date类型", str));
            }
        }

        /// <summary>
        ///  将字符串解析为一个Date类型的值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static bool TryParse(string str, out Date date)
        {
            if (string.IsNullOrEmpty(str))
            {
                date = default(Date);
                return false;
            }
            var match = Regex.Match(str, pattern);
            if (match.Success)
            {
                date = new Date(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value));
                return true;
            }
            else
            {
                date = default(Date);
                return false;
            }
        }

        /// <summary>
        /// 将YMMDD格式的int数字转换为一个Date类型的值
        /// </summary>
        /// <param name="ymmdd"></param>
        /// <returns></returns>
        public static Date FromYMMDD(int ymmdd)
        {
            int year = ymmdd / 10000;
            int month = (ymmdd - year * 10000) / 100;
            int day = ymmdd - year * 10000 - month * 100;
            return new Date(year, month, day);
        }
        /// <summary>
        /// 将YMMDD格式的uint数字转换为一个Date类型的值
        /// </summary>
        /// <param name="ymmdd"></param>
        /// <returns></returns>
        public static Date FromYMMDD(uint ymmdd)
        {
            return FromYMMDD((int)ymmdd);
        }
        #region 运算符重载
        /// <summary>
        /// 重载运算符==
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Date a, Date b)
        {
            return a.ToYMMDD() == b.ToYMMDD();
        }
        /// <summary>
        /// 重载运算符!=
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Date a, Date b)
        {

            return !(a == b);
        }
        /// <summary>
        /// 重载运算符>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator >(Date a, Date b)
        {
            return a.ToYMMDD() > b.ToYMMDD();
        }
        /// <summary>
        /// 重载运算符<
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator <(Date a, Date b)
        {
            return a.ToYMMDD() < b.ToYMMDD();
        }
        /// <summary>
        /// 重载运算符>=
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator >=(Date a, Date b)
        {
            return a.ToYMMDD() >= b.ToYMMDD();
        }
        /// <summary>
        /// 重载运算符<=
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator <=(Date a, Date b)
        {
            return a.ToYMMDD() <= b.ToYMMDD();
        }
        #endregion

        /// <summary>
        /// 比较两个Date值
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(object other)
        {
            Date dt = (Date)other;
            return CompareTo(dt);
        }
        /// <summary>
        /// 比较两个Date值
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Date other)
        {
            return this.ToYMMDD() - other.ToYMMDD();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ToYMMDD().ToString();
        }
    }
}
