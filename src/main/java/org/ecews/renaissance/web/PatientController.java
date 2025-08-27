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

import lombok.RequiredArgsConstructor;
import org.ecews.renaissance.dtos.PatientDto;
import org.ecews.renaissance.services.ClientNumberGenerator;
import org.ecews.renaissance.services.PatientService;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.UUID;

@RestController
@RequestMapping("/api/ren/patients")
@RequiredArgsConstructor
public class PatientController {
    
    private static final Logger log = LoggerFactory.getLogger(PatientController.class);
    private final PatientService patientService;
    
    private final ClientNumberGenerator generator;
    
    @GetMapping("/generate-client-number")
    public ResponseEntity<String> generateClientNumber() {
        log.info("About to generate client number ****** ");
        return ResponseEntity.ok(generator.generateClientNumber());
    }
    
    @GetMapping
    public List<PatientDto> getAll() {
        return patientService.getAllPatients();
    }
    
    @GetMapping("/{id}")
    public PatientDto getById(@PathVariable UUID id) {
        return patientService.getPatientById(id);
    }
    
    @GetMapping("/client-number/{clientNumber}")
    public List<PatientDto> getByClientNumber(@PathVariable String clientNumber) {
        return patientService.getPatientByClientNumber(clientNumber);
    }
    
    @PostMapping
    public PatientDto create(@RequestBody PatientDto dto) {
        return patientService.createPatient(dto);
    }
    
    @PutMapping("/{id}")
    public PatientDto update(@PathVariable UUID id, @RequestBody PatientDto dto) {
        return patientService.updatePatient(id, dto);
    }
    
    @DeleteMapping("/{id}")
    public void archive(@PathVariable UUID id) {
        patientService.archivePatient(id);
    }
}
