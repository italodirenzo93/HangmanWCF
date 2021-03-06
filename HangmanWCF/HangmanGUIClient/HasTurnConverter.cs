﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace HangmanGUIClient
{
    [ValueConversion(typeof(string), typeof(bool))]
    class HasTurnConverter : IValueConverter
    {
        public static HasTurnConverter Instance = new HasTurnConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? hasTurn = (bool?)value;
            if (hasTurn == true)
            {
                return "Green";
            }
            else if (hasTurn == false)
            {
                return "Blue";
            }
            else
                return "Red";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Cannot convert back");
        }
    }
}
