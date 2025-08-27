jest.mock('../list/tutorial-list.component');

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TutorialDetailsComponent } from "./tutorial.details.component";
import { MatButtonModule } from '@angular/material/button';
import { provideRouter, RouterLink } from '@angular/router';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ReactiveFormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { FuseAlertComponent } from '@mattae/angular-shared';
import { getTranslocoModule } from '../../../../transloco-testing.module';
import { TutorialService } from '../../tutorial.service';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TutorialListComponent } from '../list/tutorial-list.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

describe('TutorialDetailsComponent', () => {
    let component: TutorialDetailsComponent;
    let fixture: ComponentFixture<TutorialDetailsComponent>;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                MatButtonModule,
                RouterLink,
                getTranslocoModule(),
                MatCheckboxModule,
                MatTooltipModule,
                ReactiveFormsModule,
                MatInputModule,
                MatIconModule,
                FuseAlertComponent,
                BrowserAnimationsModule
            ],
            providers:[
                provideHttpClient(),
                provideHttpClientTesting(),
                TutorialService,
                provideRouter([]),
                TutorialListComponent
            ]
        });
        fixture = TestBed.createComponent(TutorialDetailsComponent);
        component = fixture.componentInstance;
    });

    it('should create the component', () => {
        expect(component).toBeTruthy();
    });

    it('should toggle edit mode', () => {
        expect(component.editMode).toBe(false);
        component.toggleEditMode();
        expect(component.editMode).toBe(true);
    });

});
