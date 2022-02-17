export interface IXeroAuthorization {
  getAuthorizationBaseUri: string;
  responseType: string;
  codeChallengeMethod: string;
  clientId: string;
  scope: string;
  redirectUri: string;
  state: string;
  codeChallenge: string;
  generatedUri: string;
  postTokenBaseUri: string;
  grantType: string;
  code: string;
  codeVerifier: string;
  idToken: string;
  accessToken: string;
  expiresIn: number;
  expiresDateTime: string;
  tokenType: string;
  refreshToken: string;
  xeroTenantId: string;
  xeroTenantName: string;
  xeroBillsApiUri: string;
  xeroBillsUriQueryParameters: string;
  xeroAccountsApiUri: string;
}

export interface IXeroTenant {
  id: string;
  tenantId: string;
  tenantType: string;
  tenantName: string;
}

export interface IXeroBill {
  type: string;
  invoiceID: string;
  invoiceNumber: string;
  reference: string;
  amountDue: number;
  amountPaid: number;
  amountCredited: number;
  isDiscounted: boolean;
  contact: IXeroContact;
  dateString: string;
  date: Date;
  dueDateString: string;
  dueDate: Date;
  status: string;
  lineAmountTypes: string;
  lineItems: IXeroLineItem[];
  subTotal: number;
  totalTax: number;
  total: number;
  totalInWords: string;
  updatedDateUTC: Date;
  currencyCode: string;
  includeInImport: boolean;
  searchMatched: boolean;
  importStatus: string;
  particulars: string;
}

export interface IXeroLineItem {
  description: string;
  unitAmount: number;
  taxType: string;
  taxAmount: number;
  lineAmount: number;
  quantity: number;
  lineItemID: string;
  accountCode: string;
}

export interface IXeroContact {
  contactID: string;
  name: string;
}

export interface ICheckManagerVoucher {
  id: number;
  check_number: string;
  payee: string;
  particulars: string;
  particulars_amount: number;
  remarks: string;
  amount_in_words: string;
  prepared_by: string;
  approved_by: string;
  posted_by: string;
  created_at: Date;
  updated_at: Date;
  items: ICheckManagerItem[];
}

export interface ICheckManagerItem {
  id: number;
  voucher_id: number;
  account: string;
  account_title: string;
  debit: number;
  credit: number;
  created_at: Date;
  updated_at: Date;
}
