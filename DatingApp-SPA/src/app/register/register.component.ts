import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { FormGroup, FormControl, Validators, FormBuilder } from '@angular/forms';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker/public_api';
import { User } from '../_models/user';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {

  // @Input() valuesFromHome: any;
  @Output() cancelRegister = new EventEmitter();
  // change S12.132
  // model: any = {};
  user: User;
  registerForm: FormGroup;
  bsConfig: Partial<BsDatepickerConfig>;

  constructor(private authService: AuthService,
              private alertify: AlertifyService,
              private router: Router,
              private fb: FormBuilder) { }

  ngOnInit() {
    this.bsConfig = {
      containerClass: 'theme-dark-blue'
    };
    this.createRegisterForm();
  }

  createRegisterForm() {
    // replaced in S12.128 by ng Form Builder code below
    // this.registerForm = new FormGroup({
    //   // first FormControl param is form state. can put dummy string or empty
    //   // string is used as default value for the edit field
    //   // username: new FormControl('A.username', Validators.required),
    //   username: new FormControl('', Validators.required),
    //   password: new FormControl('', [Validators.required, Validators.minLength(8), Validators.maxLength(16)]),
    //   confirmPassword: new FormControl('', Validators.required)
    // }, this.passwordMatchValidator);
      this.registerForm = this.fb.group({
        gender: ['male'],
        username: ['', Validators.required],
        knownAs: ['', Validators.required],
        dateOfBirth: [null, Validators.required],
        city: ['', Validators.required],
        country: ['', Validators.required],
        password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(16)]],
        confirmPassword: ['', Validators.required]
      }, {validator: this.passwordMatchValidator});
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('password').value === g.get('confirmPassword').value ? null : {mismatch: true};
  }

  register() {
     if (this.registerForm.valid) {
    //   // assign(target,....sources)
       this.user = Object.assign({}, this.registerForm.value);
       this.authService.register(this.user).subscribe(() => {
         this.alertify.success('Registration successful');
       }, error => {
         this.alertify.error(error);
       }, () => {
         this.authService.login(this.user).subscribe(() => {
           this.router.navigate(['/members']);
         });
       });
     }
  }

  cancel() {
    this.cancelRegister.emit(false);
    // console.log('cancelled');
    this.alertify.message('cancelled');
  }

}
