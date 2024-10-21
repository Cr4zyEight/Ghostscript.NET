namespace Ghostscript.NET.FacturX.ZUGFeRD
{

    /// <summary>
    /// **********************************************************************
    /// 
    /// Copyright 2019 by ak on 12.04.19.
    /// 
    /// Use is subject to license terms.
    /// 
    /// Licensed under the Apache License, Version 2.0 (the "License"); you may not
    /// use this file except in compliance with the License. You may obtain a copy
    /// of the License at http://www.apache.org/licenses/LICENSE-2.0.
    /// 
    /// Unless required by applicable law or agreed to in writing, software
    /// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
    /// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    /// 
    /// See the License for the specific language governing permissions and
    /// limitations under the License.
    /// 
    /// *********************************************************************** 
    /// </summary>


    public interface IZugFeRdTradeSettlementPayment : IZugFeRdTradeSettlement
    {

        /// <summary>
        /// get payment information text. e.g. Bank transfer
        /// </summary>
        /// <returns> payment information text </returns>
        string GetOwnPaymentInfoText()
        {
            return null;
        }

        /// <summary>
        /// BIC of the sender
        /// </summary>
        /// <returns> the BIC code of the recipient sender's bank </returns>
        string GetOwnBic()
        {
            return null;
        }


        /// <summary>
        /// IBAN of the sender
        /// </summary>
        /// <returns> the IBAN of the invoice sender's bank account </returns>
        string GetOwnIban()
        {
            return null;
        }

        /// <summary>
        ///*
        /// Account name
        /// </summary>
        /// <returns> the name of the account holder (if not identical to sender) </returns>
        string GetAccountName()
        {
            return null;
        }


        public string GetSettlementXml()
        {
            string accountNameStr = "";
            if (GetAccountName() != null)
            {
                accountNameStr = "<ram:AccountName>" + XmlTools.EncodeXml(GetAccountName()) + "</ram:AccountName>\n";

            }

            string xml = "			<ram:SpecifiedTradeSettlementPaymentMeans>\n" + "				<ram:TypeCode>58</ram:TypeCode>\n" + "				<ram:Information>SEPA credit transfer</ram:Information>\n" + "				<ram:PayeePartyCreditorFinancialAccount>\n" + "					<ram:IBANID>" + XmlTools.EncodeXml(GetOwnIban()) + "</ram:IBANID>\n";
            xml += accountNameStr;
            xml += "				</ram:PayeePartyCreditorFinancialAccount>\n" + "				<ram:PayeeSpecifiedCreditorFinancialInstitution>\n" + "					<ram:BICID>" + XmlTools.EncodeXml(GetOwnBic()) + "</ram:BICID>\n" + "				</ram:PayeeSpecifiedCreditorFinancialInstitution>\n" + "			</ram:SpecifiedTradeSettlementPaymentMeans>\n";
            return xml;
        }


        /* I'd love to implement getPaymentXML() and put <ram:DueDateDateTime> there because this is where it belongs
         * unfortunately, the due date is part of the transaction which is not accessible here :-(
         */


    }

}