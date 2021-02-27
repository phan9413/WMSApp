using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace WMSApp.Models.Setting
{
    public enum FieldsType
    {
        docSeries,
        bizPartner,
        contactPerson,
        currency,
        salesEmp,
        postDate,
        deliveryDate,
        docDate,
        remarks,
        custRef,
        shipTo,
        billTo,
        items,
        totalBeforeDis,
        disByPercent,
        disByvalue,
        tax,
        total,
        attachment,

        // for the quantity entry screen
        QeItemCode,
        QeqtyItemName,
        QeWarehouse,
        QeDeliveryDate,
        QeQuantity,
        QeUom,
        QePriceList,
        QeUnitPrice,
        QeTotalBforeDis,
        QeDisByPercent,
        QeDisByVal,
        QeTax,
        QeTaxAmt,
        QeGrossPrice,
        QeTotal,

        pyDocSeries,
        pyBpCode,
        pyBpName,
        pyDocDate,
        pyDueDate,
        pyTaxDate,
        pyRef,
        pyComments,
        pyjrnlMemo,
        pySelectedInvs,
        pyMeansCheque,
        pyMeansCash,
        pyTotalAmtDue
    }

    public class zsCommDocFields : INotifyPropertyChanged
    {
        public int id { get; set; }
        public string title { get; set; }
        string _value;
        public string value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(value));
                }
            }
        }
        public bool isVisible { get; set; }
        public bool isRightArrowVisible { get; set; }
        public FieldsType fieldType { get; set; }
        public bool isExpandValueHideTitle { get; set; }
        public object Tag { get; set; } // for holding generic value  object

        // for biz partner - transactio page 
        public bool isShowDocNum { get; set; }

        string _noOfOpenDoc;
        public string noOfOpenDoc
        {
            get => _noOfOpenDoc;
            set
            {
                if (_noOfOpenDoc != value)
                {
                    _noOfOpenDoc = value;
                    OnPropertyChanged(nameof(noOfOpenDoc));
                }
            }
        }

        public bool isHighLightCellColor { get; set; } = false;

        // mandatory setting, 20200420T946
        public bool isMandatoryField { get; set; }
        public bool isAction { get; set; } //<-- use to invike the page
        public Keyboard keyboard { get; set; }

        public Color cellBackgroundColor
        {
            get => Color.White;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Handle the on property changed, value update to screen
        /// </summary>
        /// <param name="propertyname"></param>
        public void OnPropertyChanged(string propertyname) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
    }
}
