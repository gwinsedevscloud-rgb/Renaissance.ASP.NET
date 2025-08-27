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

import lombok.RequiredArgsConstructor;
import org.ecews.renaissance.domain.Ancillary;
import org.ecews.renaissance.domain.Patient;
import org.ecews.renaissance.dtos.AncillaryDto;
import org.ecews.renaissance.repositories.AncillaryRepository;
import org.ecews.renaissance.repositories.PatientRepository;
import org.ecews.renaissance.utils.Utils;
import org.springframework.stereotype.Service;

import java.time.LocalDateTime;
import java.util.List;
import java.util.UUID;

@Service
@RequiredArgsConstructor
public class AncillaryServiceService {

    private final AncillaryRepository ancillaryServiceRepository;
    private final PatientRepository patientRepository;
    private final Utils utils;

    public Ancillary save(AncillaryDto dto) {
        Patient patient = patientRepository.findById(dto.getPatientId())
                .orElseThrow(() -> new IllegalArgumentException("Invalid patient ID"));

        Ancillary service = new Ancillary();
        service.setPatient(patient);
        service.setServices(dto.getServices());
        service.setPregnancyStatus(dto.getPregnancyStatus());
        service.setCreatedBy(utils.getCurrentUserName());
        service.setCreatedDate(LocalDateTime.now());
        service.setUpdatedBy(utils.getCurrentUserName());
        service.setUpdatedDate(LocalDateTime.now());

        return ancillaryServiceRepository.save(service);
    }

    public Ancillary update(UUID id, AncillaryDto dto) {
        Ancillary service = ancillaryServiceRepository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("Service not found"));

        service.setServices(dto.getServices());
        service.setPregnancyStatus(dto.getPregnancyStatus());
        service.setUpdatedBy(utils.getCurrentUserName());
        service.setUpdatedDate(LocalDateTime.now());

        return ancillaryServiceRepository.save(service);
    }

    public void delete(UUID id) {
        ancillaryServiceRepository.deleteById(id);
    }

    public List<Ancillary> findAll() {
        return ancillaryServiceRepository.findAll();
    }

    public Ancillary findById(UUID id) {
        return ancillaryServiceRepository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("Service not found"));
    }

    public List<Ancillary> findByPatientId(UUID patientId) {
        return ancillaryServiceRepository.findByPatientId(patientId);
    }
}
