import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
@Component({
  selector: 'app-not-found',
  imports: [RouterLink],
  template:
    '<main class="not-found"><span>404</span><h1>Page not found</h1><p>The page you are looking for does not exist.</p><a routerLink="/dashboard">Back to dashboard</a>',
  styles: [
    `
      .not-found {
        min-height: 100vh;
        display: flex;
        align-items: center;
        justify-content: center;
        flex-direction: column;
        font-family: Arial, sans-serif;
        color: #24394a;
      }
      .not-found > span {
        font-size: 54px;
        color: #70917b;
        font-weight: bold;
      }
      .not-found h1 {
        margin: 10px 0;
      }
      .not-found p {
        color: #82909a;
      }
      .not-found a {
        color: #416e50;
        margin-top: 15px;
      }
    `,
  ],
})
export class NotFoundPage {}
