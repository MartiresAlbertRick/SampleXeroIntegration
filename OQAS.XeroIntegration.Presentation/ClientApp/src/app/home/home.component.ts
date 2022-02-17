import { Component, Inject, Input, OnInit, TemplateRef } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { IXeroAuthorization, IXeroBill, IXeroLineItem, ICheckManagerVoucher, ICheckManagerItem } from '../models/entities';
import { BsModalService } from 'ngx-bootstrap/modal';
import { BsModalRef } from 'ngx-bootstrap/modal/bs-modal-ref.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent implements OnInit {
  xeroAuthorization: IXeroAuthorization;
  xeroBills: IXeroBill[];
  selectedXeroBill: IXeroBill;
  mainCheckBox: boolean = false;
  modalRef: BsModalRef;
  billsLoaded: boolean = false;
  searchKeyword: string;
  disableImportButton: boolean = true;

  constructor(private http: HttpClient, private modalService: BsModalService, @Inject('BASE_URL') private baseUrl: string) { }

  ngOnInit() {
    this.http.get<IXeroAuthorization>(this.baseUrl + 'api/cvx/get-configuration')
      .subscribe(
        result => {
          this.xeroAuthorization = result;
        },
        error => {
          console.error(error);
          alert(error.error);
        });
  }

  requestDataFromXero() {
    this.disableImportButton = true;
    this.mainCheckBox = false;
    this.xeroBills = [];

    this.http.get<IXeroBill[]>(this.baseUrl + 'api/cvx/get-bills')
      .subscribe(
        result => {
          console.log(result);
          this.xeroBills = result;
          this.xeroBills.forEach(bill => {
            bill.searchMatched = true;
            bill.importStatus = "NOT IMPORTED";
            bill.particulars = bill.lineItems[0].description;
          });
          this.billsLoaded = true;
          this.modalRef.hide();
        },
        error => {
          this.modalRef.hide();
          console.error(error);
          alert(error.error);
        });
  }

  search() {
    this.xeroBills.forEach(bill => {
      if (bill.contact.name.includes(this.searchKeyword)) {
        bill.searchMatched = true;
      } else {
        bill.searchMatched = false;
      }
    });
  }

  checkUncheckAll() {
    this.xeroBills.forEach(bill => {
      if (bill.importStatus == "NOT IMPORTED" && bill.searchMatched) {
        bill.includeInImport = this.mainCheckBox
      }
    });

    if (this.mainCheckBox) {
      this.disableImportButton = false;
    } else {
      this.disableImportButton = true;
    }
  }

  singleRecordCheckUncheck() {
    let counter = 0;
    this.xeroBills.forEach(bill => {
      if (bill.includeInImport) {
        counter++;
      }
    });
    if (counter > 0) {
      this.disableImportButton = false;
    } else {
      this.disableImportButton = true;
    }
  }

  selectBill(selected: IXeroBill) {
    this.selectedXeroBill = selected;
  }

  openModal(template: TemplateRef<any>) {
    this.modalRef = this.modalService.show(template, { backdrop: 'static', keyboard: false, class: 'modal-xl' });
  }

  openSpinner(template: TemplateRef<any>) {
    this.modalRef = this.modalService.show(template, { backdrop: 'static', keyboard: false });
  }

  importSingleRecord(selected: IXeroBill) {
    if (confirm("import this record?")) { 
      let vouchers: ICheckManagerVoucher[] = [];
      this.xeroBills.forEach(bill => {
        if (bill.invoiceNumber == selected.invoiceNumber) {
          vouchers.push(this.convertXeroBillToCheckManagerVoucher(bill));
          console.log(vouchers);
          this.http.post<ICheckManagerVoucher[]>(this.baseUrl + 'api/cvx/import-voucher', vouchers)
            .subscribe(
              result => {
                console.log(result);
                selected.includeInImport = false;
                selected.importStatus = "IMPORTED";
                alert('successfully imported');
              },
              error => {
                console.error(error);
                alert(error.error);
              });
        }
      });
    }
  }

  importMultipleRecords() {
    let vouchers: ICheckManagerVoucher[] = [];
    this.xeroBills.forEach(bill => {
      if (bill.includeInImport && bill.importStatus != "IMPORTED") {
        vouchers.push(this.convertXeroBillToCheckManagerVoucher(bill));
      }
    });
    console.log(vouchers);
    this.http.post<ICheckManagerVoucher[]>(this.baseUrl + 'api/cvx/import-voucher', vouchers)
      .subscribe(
        result => {
          console.log(result);
          alert(result.length + '/' + vouchers.length + ' successfully imported');
          this.xeroBills.forEach(bill => {
            if (bill.includeInImport && bill.importStatus != "IMPORTED") {
              bill.includeInImport = false;
              bill.importStatus = "IMPORTED";
            }
          });
          this.disableImportButton = true;
          this.mainCheckBox = false;
        },
        error => {
          console.error(error);
          alert(error.error);
        });
  }

  assignParticulars(lineItem: IXeroLineItem) {
    this.selectedXeroBill.particulars = lineItem.description;
  }

  convertXeroBillToCheckManagerVoucher(bill: IXeroBill): ICheckManagerVoucher {
    let voucher: ICheckManagerVoucher = {
      amount_in_words: bill.totalInWords,
      particulars: bill.particulars,
      particulars_amount: bill.total,
      created_at: bill.date,
      payee: bill.contact.name,
      items: [],
      id: 0,
      check_number: null,
      approved_by: null,
      remarks: null,
      posted_by: null,
      prepared_by: null,
      updated_at: null
    };

    bill.lineItems.forEach(lineItem => {
      let item: ICheckManagerItem = {
        account_title: lineItem.accountCode,
        created_at: bill.date,
        credit: 0,
        debit: 0,
        account: null,
        updated_at: null,
        id: 0,
        voucher_id: 0
      };
      if (lineItem.lineAmount >= 0) {
        item.debit = lineItem.lineAmount;
      } else {
        item.credit = lineItem.lineAmount;
      }
      voucher.items.push(item);
    });
    let cashInBank: ICheckManagerItem = {
      id: 0,
      voucher_id: 0,
      account: null,
      account_title: '<span style="margin-left:20px;">Cash in bank</span>',
      debit: 0,
      credit: bill.total,
      created_at: bill.date,
      updated_at: null
    };
    voucher.items.push(cashInBank);
    return voucher;
  } 
}
