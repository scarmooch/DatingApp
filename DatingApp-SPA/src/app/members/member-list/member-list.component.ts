import { Component, OnInit } from '@angular/core';
import { User } from '../../_models/user';
import { UserService } from '../../_services/user.service';
import { AlertifyService } from '../../_services/alertify.service';
import { ActivatedRoute } from '@angular/router';
import { Pagination, PaginatedResult } from 'src/app/_models/pagination';
import { FileUploadModule } from 'ng2-file-upload';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  users: User[];
  user: User = JSON.parse(localStorage.getItem('user'));
  genderList = [{value: 'male', display: 'Males'}, {value: 'female', display: 'Females'}];
  userParams: any = {};
  pagination: Pagination;

  constructor(private userService: UserService,
              private alertify: AlertifyService,
              private route: ActivatedRoute) { }

  ngOnInit() {
    // this.loadUsers();
    const key = 'users'; // 'user' matches routes.ts resolve: {users: MemberListResolver}
    this.route.data.subscribe(data => {
      this.users = data[key].result;
      this.pagination = data[key].pagination;
    });

    this.initFilters();
    this.initSorting();
  }


  pageChanged(event: any): void {
    this.pagination.currentPage = event.page;
    // this.alertify.message('current page: ' + this.pagination.currentPage);
    this.loadUsers();
  }

  initSorting() {
    this.userParams.orderBy = 'lastActive';
  }

  initFilters() {
    this.userParams.gender = this.user.gender === 'female' ? 'male' : 'female';
    this.userParams.minAge = 18;
    this.userParams.maxAge = 129;
  }

  resetFilters() {
    this.initFilters();
    this.loadUsers();
  }

  loadUsers() {
    this.userService.getUsers(this.pagination.currentPage, this.pagination.itemsPerPage, this.userParams)
      .subscribe((res: PaginatedResult<User[]>) => {
      this.users = res.result;
      this.pagination = res.pagination;
    }, error => {
      this.alertify.error(error);
    });
  }
}
