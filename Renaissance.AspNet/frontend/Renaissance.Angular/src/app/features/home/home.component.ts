import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
    selector: 'app-home',
    imports: [RouterLink, MatButtonModule, MatIconModule],
    templateUrl: './home.component.html'
})
export class HomeComponent {
    stats = [
        { label: 'Clinical modules', value: '8', icon: 'heroicons_outline:squares-2x2' },
        { label: 'Departments', value: '6+', icon: 'heroicons_outline:building-office-2' },
        { label: 'API connected', value: 'Live', icon: 'heroicons_outline:signal' }
    ];

    features = [
        {
            title: 'Client Registry',
            icon: 'heroicons_outline:users',
            colorClass: '',
            link: '/clients',
            description: 'Register and search clients with auto-generated client numbers.'
        },
        {
            title: 'Clinical Dashboard',
            icon: 'heroicons_outline:clipboard-document-list',
            colorClass: 'blue',
            link: '/clients',
            description: 'Unified view of triage, consultations, pharmacy, and more.'
        },
        {
            title: 'Multi-department Care',
            icon: 'heroicons_outline:building-office-2',
            colorClass: 'violet',
            link: '/clients',
            description: 'Laboratory, dental, ancillary, and eye care modules.'
        }
    ];
}
