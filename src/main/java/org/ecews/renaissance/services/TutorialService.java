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
package org.ecews.renaissance.services;

import org.ecews.renaissance.domain.Tutorial;
import org.ecews.renaissance.repositories.TutorialRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.Optional;

@RequiredArgsConstructor
@Service
public class TutorialService {
    
    private final TutorialRepository tutorialRepository;
    
    public List<Tutorial> getAllTutorials() {
        return tutorialRepository.findAll();
    }
    
    public Optional<Tutorial> getTutorial(Long id) {
        return tutorialRepository.findById(id);
    }
    
    public Tutorial saveTutorial(Tutorial tutorial) {
        return tutorialRepository.save(tutorial);
    }
    
    public void deleteTutorial(Long id) {
        tutorialRepository.findById(id).ifPresent(tutorialRepository::delete);
    }
    
    public List<Tutorial> findByTitle(String title) {
        return tutorialRepository.findByTitleLikeIgnoreCase("%" + title + "%");
    }
    
    public void deleteAll() {
        tutorialRepository.deleteAll();
    }
}
