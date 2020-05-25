import { Injectable } from '@angular/core';
// import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { CanActivate, Router } from '@angular/router';
// import { Observable } from 'rxjs';

import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';


@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor( private authService: AuthService,
               private router: Router,
               private alertify: AlertifyService)
  {}
  // canActivate(
    // next: ActivatedRouteSnapshot,
    // state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    // return true;
  // }
  canActivate(): boolean {
    // if we are logged in
    if (this.authService.loggedIn())
    {
      return true;
    }
    this.alertify.error('You shall not pass!!!');
    this.router.navigate(['/home']);
    return false;
  }
}