namespace Ghostscript.NET.FacturX.ZUGFeRD;

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
public interface IZUGFeRDTradeSettlementPayment : IZUGFeRDTradeSettlement
{
    /// <summary>
    /// get payment information text. e.g. Bank transfer
    /// </summary>
    /// <returns> payment information text </returns>
    string GetOwnPaymentInfoText();

    /// <summary>
    /// BIC of the sender
    /// </summary>
    /// <returns> the BIC code of the recipient sender's bank </returns>
    string GetOwnBic();


    /// <summary>
    /// IBAN of the sender
    /// </summary>
    /// <returns> the IBAN of the invoice sender's bank account </returns>
    string GetOwnIban();

    /// <summary>
    ///*
    /// Account name
    /// </summary>
    /// <returns> the name of the account holder (if not identical to sender) </returns>
    string GetAccountName();


    public string GetSettlementXml();


    /* I'd love to implement getPaymentXML() and put <ram:DueDateDateTime> there because this is where it belongs
     * unfortunately, the due date is part of the transaction which is not accessible here :-(
     */
}