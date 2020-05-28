import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { FormGroup, FormControl, Validators, FormBuilder } from '@angular/forms';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {

  // @Input() valuesFromHome: any;
  @Output() cancelRegister = new EventEmitter();
  model: any = {};
  registerForm: FormGroup;

  constructor(private authService: AuthService,
              private alertify: AlertifyService,
              private fb: FormBuilder) { }

  ngOnInit() {
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
      username: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(16)]],
      confirmPassword: ['', Validators.required]
    }, {validator: this.passwordMatchValidator});
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('password').value === g.get('confirmPassword').value ? null : {mismatch: true};
  }

  register() {
    /* disable for now to work on creating reactive form
    this.authService.register(this.model).subscribe(() => {
       // console.log('registration successful');
       this.alertify.success('registration successful');
    }, error => {
      // console.log(error);
      this.alertify.error(error);
    });
    */
   console.log(this.registerForm.value);
  }

  cancel() {
    this.cancelRegister.emit(false);
    // console.log('cancelled');
    this.alertify.message('cancelled');
  }

}
