/*
* MIT License
*
* Copyright (c) 2025 [AUTHOR OR ORGANIZATION]
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
package org.ecews.renaissance.web;

import org.ecews.renaissance.domain.Tutorial;
import org.ecews.renaissance.services.TutorialService;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/api/ren/tutorials")
@RequiredArgsConstructor
public class TutorialResource {
    
    private final TutorialService tutorialService;
    
    @GetMapping("/{id}")
    public ResponseEntity<Tutorial> getTutorial(@PathVariable Long id) {
        return ResponseEntity.of(tutorialService.getTutorial(id));
    }
    
    @GetMapping
    public List<Tutorial> getAllTutorials() {
        return tutorialService.getAllTutorials();
    }
    
    @PostMapping
    public Tutorial saveTutorial(@RequestBody Tutorial tutorial) {
        return tutorialService.saveTutorial(tutorial);
    }
    
    @PutMapping
    public Tutorial updateTutorial(@RequestBody Tutorial tutorial) {
        return tutorialService.saveTutorial(tutorial);
    }
    
    @DeleteMapping("/{id}")
    public void deleteTutorial(@PathVariable Long id) {
        tutorialService.deleteTutorial(id);
    }
    
    @DeleteMapping
    public void deleteAll() {
        tutorialService.deleteAll();
    }
    
    @GetMapping("/title")
    public List<Tutorial> findByTitle(@RequestParam String title) {
        return tutorialService.findByTitle(title);
    }
}
