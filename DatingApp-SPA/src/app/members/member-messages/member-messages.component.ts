import { Component, OnInit, Input } from '@angular/core';
import { Message } from 'src/app/_models/message';
import { AuthService } from 'src/app/_services/auth.service';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { UserService } from 'src/app/_services/user.service';
import { tap } from 'rxjs/operators';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
  // id of the detailed profile that we are on
  @Input() recipientId: number;
  messages: Message[];
  newMessage: any = {};

  constructor(private authService: AuthService,
              private userService: UserService,
              private alertify: AlertifyService) { }

  ngOnInit() {
    this.loadMessages();
  }

  loadMessages() {
    // use + to convert to a number, so that type comparison === works
    const currentUserId = +this.authService.decodedToken.nameid;
    this.userService.getMessageThread(this.authService.decodedToken.nameid, this.recipientId)
      .pipe(
        tap(messages => {

          // for (let i = 0; i < messages.length; i++) { ==> this gives tslint warning:
          // Expected a 'for-of' loop instead of a 'for' loop with this simple iteration (prefer-for-of)
          for (let i = 0; i < messages.length; i++) {
            if (messages[i].isRead === false && messages[i].recipientId === currentUserId) {
              this.userService.markAsRead(messages[i].id, currentUserId);
            }
          }
        })
      )
      .subscribe(messages => {
        this.messages = messages;
    }, error => {
        this.alertify.error(error);
    });
  }

  sendMessage() {
    this.newMessage.recipientId = this.recipientId;
    this.userService.sendMessage(this.authService.decodedToken.nameid, this.newMessage)
      .subscribe((message: Message) => {
        // debugger;
        this.messages.unshift(message);
        this.newMessage.content = '';
    }, error => {
      this.alertify.error(error);
    });
  }
}
