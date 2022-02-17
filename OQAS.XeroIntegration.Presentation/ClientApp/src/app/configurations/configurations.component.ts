import { Component, Inject, Input, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { IXeroAuthorization, IXeroTenant } from '../models/entities';

@Component({
  selector: 'app-configurations-component',
  templateUrl: './configurations.component.html'
})
export class ConfigurationsComponent implements OnInit {
  xeroAuthorization: IXeroAuthorization;
  xeroTenants: IXeroTenant[];

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }

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

  generateUri() {
    this.http.post<IXeroAuthorization>(this.baseUrl + 'api/cvx/generate-uri', this.xeroAuthorization)
      .subscribe(
        result => {
          this.xeroAuthorization = result;
        },
        error => {
          console.error(error);
          alert(error.error);
        });
  }

  generateToken() {
    this.http.post<IXeroAuthorization>(this.baseUrl + 'api/cvx/generate-tokens', this.xeroAuthorization)
      .subscribe(
        result => {
          this.xeroAuthorization = result;
        },
        error => {
          console.error(error);
          alert(error.error);
        });
  }

  getXeroTenants() {
    this.http.post<IXeroTenant[]>(this.baseUrl + 'api/cvx/get-xero-tenants', this.xeroAuthorization)
      .subscribe(
        result => {
          this.xeroTenants = result;
        },
        error => {
          console.error(error);
          alert(error.error);
        });
  }

  assignTenantId(xeroTenant: IXeroTenant) {
    this.xeroAuthorization.xeroTenantId = xeroTenant.tenantId;
    this.xeroAuthorization.xeroTenantName = xeroTenant.tenantName;
  }

  saveConfiguration() {
    this.http.post<IXeroAuthorization>(this.baseUrl + 'api/cvx/save-configuration', this.xeroAuthorization)
      .subscribe(
        result => {
          this.xeroAuthorization = result;
          alert('successfully saved');
        },
        error => {
          console.error(error);
          alert(error.error);
        });
  }
}

